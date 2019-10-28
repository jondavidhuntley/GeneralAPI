using GeneralAPI.Services.DataServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralAPI.Controllers
{
    /// <summary>
    /// RawDocumentController
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RawDocumentController : DocumentControllerBase
    {
        private readonly IReportDataService _reportDataService;
        private readonly IClientTokenHandler _clientTokenHandler;
        private readonly IDocumentStoreSettings _documentSettings;
        private readonly ILogger<RawDocumentController> _logger;

        /// <summary>
        /// Raw Input Sheet Controller
        /// </summary>
        /// <param name="reportDataService">Report Data Service</param>
        /// <param name="clientTokenHandler">Auth0 Token Service</param>
        /// <param name="documentSettings">Document Settings</param>
        /// <param name="serviceProvider">Service Provider</param>
        /// <param name="logger">Event Logger</param>
        public RawDocumentController(IReportDataService reportDataService,
                                        IClientTokenHandler clientTokenHandler,
                                        IDocumentStoreSettings documentSettings,
                                        IServiceProvider serviceProvider,
                                        ILogger<RawDocumentController> logger)
            : base(serviceProvider, clientTokenHandler, documentSettings)
        {
            _reportDataService = reportDataService;
            _clientTokenHandler = clientTokenHandler;
            _documentSettings = documentSettings;
            _logger = logger;
        }

        /// <summary>
        /// GetRawInputControlSheetById
        /// </summary>
        /// <param name="schemaId">schemaId</param>
        /// <param name="documentId">documentId</param>
        /// <returns>document</returns>
        [HttpGet]
        [Route("{schemaId}/{documentId}")]
        [SwaggerOperation(Summary = "Recovers RAW INPUT CONTROL SHEET using GCP DocumentId - Can be called direct from Azure Function")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRawDocumentByIdAsync(string schemaId, string documentId)
        {
            try
            {
                string currency = "native";

                return await GetDocumentAsync(schemaId, currency, documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Recover Document with Id:{documentId} -> Error Message:{ex.Message}");
            }

            return NotFound($"No Report Found for Document Id:{documentId}");
        }

        /// <summary>
        /// Get Document fro a Specific Airline
        /// </summary>
        /// <param name="schemaId">schemaId</param>
        /// <param name="iaocCode">Airline IAOC Code</param>
        /// <param name="reportPeriod">Reporting Period - Annual, Q1, Q2, Q3, Q4, H1, H2</param>
        /// <param name="year">Report Year</param>
        /// <param name="currency">Currency Code</param>
        /// <returns>Report Document Model</returns>
        [HttpGet]
        [Route("{schemaId}/{iaocCode}/{reportPeriod}/{year}/{currency}")]
        [SwaggerOperation(Summary = "Recovers Report Sheet for an Airline in the requested currency")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Authorize("read:documents")]
        public async Task<IActionResult> GetReportDocumentAsync(string schemaId, string iaocCode, ReportPeriod reportPeriod, string year, string currency = "native")
        {
            try
            {
                currency = currency.Trim().ToLowerInvariant();
                var adjustedSchema = schemaId;
                if (currency != "native")
                {
                    adjustedSchema += "-" + currency.ToLowerInvariant();
                }

                // Recover Document Details from DB
                var report = _reportDataService.GetReportDetail(iaocCode, reportPeriod, schemaId, Convert.ToInt32(year, CultureInfo.InvariantCulture));
                if (report != null && !string.IsNullOrEmpty(report.Airline))
                {
                    return await GetDocumentAsync(schemaId, currency, report.DocumentId.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Recover Document for airline:{iaocCode}, period:{reportPeriod}, type:{schemaId} and year:{year} -> Error Message:{ex.Message}");
            }

            return NotFound($"No Report Found for airline:{iaocCode}, period:{reportPeriod}, type:{schemaId} and year:{year}");
        }

        /// <summary>
        /// Stores RAW Document
        /// </summary>
        /// <param name="schemaId">schemaId</param>
        /// <param name="postedDocument">Share Info Data Sheet</param>
        /// <returns>Action Result</returns>
        [HttpPost]
        [Route("{schemaId}")]
        [SwaggerOperation(Summary = "Stores a Raw Report for an Airline in Native Currency - Extracted from Legacy SQL Server")]
        [SwaggerResponse((int)HttpStatusCode.Created)]
        [SwaggerResponse((int)HttpStatusCode.Conflict)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Authorize("store:documents")]
        public IActionResult PostRawDocument(string schemaId, [FromBody]dynamic postedDocument)
        {
            var documentHandler = GetDocumentHandler(schemaId);
            // make sure that values are valid for base data such as AirlineICAOCode, ReportPeriod, ReportDate and Currency
            var reportBaseValidation = new ReportBaseValidation(postedDocument);
            if (!TryValidateModel(reportBaseValidation))
            {
                return BadRequest(new BadRequestObjectResult(ModelState));
            }

            // Serialize Model to JSON
            var json = JsonConvert.SerializeObject(postedDocument);
            // POST Document to GCP
            var clientToken = _clientTokenHandler.GetValidToken();

            var documentResponse = documentHandler.PostNewDocument(new Uri(_documentSettings.Endpoint, schemaId).ToString(), clientToken, json, CancellationToken.None).GetAwaiter().GetResult();
            if (documentResponse.Success)
            {
                var documentId = new Guid(new Uri(documentResponse.DocumentUri).Segments.Last());
                // Save Report to report database
                var report = GetReportBaseData(json, schemaId, documentId);
                // Save Report in Database
                var saveResponse = _reportDataService.RegisterNewReport(report);
                if (saveResponse.Success)
                {
                    _logger.LogInformation($"Successfully stored new Document:{report.DocumentId} or Schema:{schemaId}");
                }

                return Created(new Uri(documentResponse.DocumentUri), documentId);
            }
            else
            {
                _logger.LogWarning($"Unprocessable Entity");
                return UnprocessableEntity(documentResponse.Content);
            }
        }
    }
}