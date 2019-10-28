using GeneralAPI.Services;
using GeneralAPI.Services.DataServices;
using GeneralAPI.Services.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralAPI.Controllers
{
    /// <summary>
    /// Document Service Administration Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IReportDataService _reportDataService;
        private readonly IClientTokenHandler _clientTokenHandler;
        private readonly IDocumentDeletionProcessor _documentDeletionProcessor;
        private readonly IDocumentUpsertProcessor _documentUpsertProcessor;
        private readonly IHistoricDataDeletionProcessor _historicDocumentProcessor;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger;

        private readonly string _gcpAserviceAPI = string.Empty;

        /// <summary>
        /// Admin Controller
        /// </summary>
        /// <param name="reportDataService">Report Data Service</param>
        /// <param name="documentDeletionProcessor">Document Deletion Service</param>
        /// <param name="documentUpsertProcessor">Document Upsert Processor</param>
        /// <param name="clientTokenHandler">GCP Token Handler</param>
        /// <param name="historicDocumentProcessor">Historic Document Processor</param>
        /// <param name="messagePublisher">Message Publisher</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="logger">Event Logger</param>
        public AdminController(IReportDataService reportDataService,
                                IDocumentDeletionProcessor documentDeletionProcessor,
                                IDocumentUpsertProcessor documentUpsertProcessor,
                                IClientTokenHandler clientTokenHandler,
                                IHistoricDataDeletionProcessor historicDocumentProcessor,
                                IMessagePublisher messagePublisher,
                                IConfiguration configuration,
                                ILogger<AdminController> logger)
        {
            _reportDataService = reportDataService;
            _documentDeletionProcessor = documentDeletionProcessor;
            _documentUpsertProcessor = documentUpsertProcessor;
            _clientTokenHandler = clientTokenHandler;
            _historicDocumentProcessor = historicDocumentProcessor;

            // var services = serviceProvider.GetServices<IMessagePublisher>();
            // _messagePublisher = services.First(s => s.GetType() == typeof(MessagePublisher) && !s.IsGCPVersion);
            // _messagePublisherGCP = services.First(s => s.GetType() == typeof(MessagePublisher) && s.IsGCPVersion);
            _messagePublisher = messagePublisher;
            _configuration = configuration;
            _logger = logger;

            _gcpAserviceAPI = configuration.GetValue<string>("GcpAudience");
        }

        /// <summary>
        /// Deletes a document
        /// </summary>
        /// <param name="schemaId">The schema id</param>
        /// <param name="documentId">The document id</param>
        [HttpDelete]
        [SwaggerOperation(Summary = "Deletes a Report Sheet for given GCP DocumentID")]
        [Route("DeleteReport/{schemaId}/{documentId}")]
        [Authorize("store:documents")]
        public void Delete(string schemaId, string documentId)
        {
            documentId = documentId.ToLowerInvariant();

            // Get GCP Auth0 Token
            var clientToken = _clientTokenHandler.GetValidToken();

            // DELETE Document from GCP
            var gcpDeletionResponse = _documentDeletionProcessor.DeleteDocumentAsync(schemaId, documentId, clientToken, CancellationToken.None).GetAwaiter().GetResult();
            if (gcpDeletionResponse == true)
            {
                // Delete Report in Database
                var response = _reportDataService.DeleteReport(documentId);
                if (response.Success)
                {
                    _logger.LogInformation($"Successfully Deleted Document with Id:{documentId}");
                }
            }
        }

        /// <summary>
        /// Deletes All Historic Records in Relation to a Completed Report
        /// Called by Listener Function on Completion of a Final Report
        /// Example Complete income-statement
        /// System will delete all historic data for the following schemas
        ///      raw-income-statement, partial-income-statement, percentage-change-income-statement and income-statement
        ///      for specified AirlineId, Report Period and Year
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period (Annual)</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Year</param>
        /// <returns>Task for completion</returns>
        [HttpDelete]
        [SwaggerOperation(Summary = "Deletes all Historic Report Sheet for given Completed Report Type")]
        [Route("DeleteReportHistory/{iaocCode}/{reportPeriod}/{reportType}/{year}")]
        [Authorize("delete:documents")]
        public async Task DeleteHistoricReportsAsync(string iaocCode, ReportPeriod reportPeriod, string reportType, int year)
        {
            _logger.LogInformation($"Called Historic Report Deletion for :{iaocCode} period:{reportPeriod} type:{reportType} year:{year}");
            await _historicDocumentProcessor.DeleteHistoricReportsAsync(iaocCode, reportPeriod, reportType, year);
            _logger.LogInformation($"Completed Historic Report Deletion for :{iaocCode} period:{reportPeriod} type:{reportType} year:{year}");
        }

        /// <summary>
        /// Publish Secondary Report Notification Artificially
        /// </summary>
        /// <param name="iaocCode">Airline ICAO Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="year">Report Year</param>
        /// <param name="message">Message</param>
        /// <returns>Success Indicator Boolean</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Publishes Secondary Report Notification for specified Airline and Reporting Period")]
        [Route("PublishSecondaryReportNotificationAsync/{iaocCode}/{reportPeriod}/{year}/{message}")]
        [Authorize("store:documents")]
        public async Task<IActionResult> PublishSecondaryReportNotificationAsync(string iaocCode, string reportPeriod, int year, string message)
        {
            var topic = _configuration.GetValue<string>("TopicSecondaryReportNotification");

            try
            {
                var notification = new ReportNotification
                {
                    AirlineICAOCode = iaocCode,
                    ReportingPeriod = reportPeriod,
                    ReportDateUTC = new DateTime(year, 1, 1),
                    Message = message
                };

                var payload = Newtonsoft.Json.JsonConvert.SerializeObject(notification);

                var response = await _messagePublisher.PublishNotificationAsync(topic, payload);

                if (response)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FAILED trying to Publish Secondary Report Notification", iaocCode, reportPeriod, message);
            }

            return BadRequest();
        }

        /// <summary>
        /// Publish Adhoc Storage Event
        /// Recover Existing Report and Upsert/Update it which will Publish a new storage event
        /// </summary>
        /// <param name="schemaId">GCP Schema Id</param>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Annual, Qn, Hn</param>
        /// <param name="year">year</param>
        /// <param name="currency">Currency</param>
        /// <returns>Ok result</returns>
        [HttpPut]
        [Route("{schemaId}/{iaocCode}/{reportPeriod}/{year}/{currency}")]
        [SwaggerOperation(Summary = "Recovers Report Sheet for an Airline in the requested currency")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Authorize("store:documents")]
        public async Task<IActionResult> PublishReportStorageNotificationAsync(string schemaId, string iaocCode, ReportPeriod reportPeriod, string year, string currency = "native")
        {
            try
            {
                currency = currency.Trim().ToLowerInvariant();

                var adjustedSchema = schemaId;
                if (currency != "native")
                {
                    // adjustedSchema += "-" + currency.ToLowerInvariant();
                }

                // Recover Document Details from DB
                var report = _reportDataService.GetReportDetail(iaocCode, reportPeriod, adjustedSchema, Convert.ToInt32(year, CultureInfo.InvariantCulture));
                if (report != null)
                {
                    var documentId = report.DocumentId.ToString();
                    var gcpToken = _clientTokenHandler.GetValidToken();

                    var documentURI = $"{_gcpAserviceAPI}{schemaId}/{documentId}";

                    // Get Stored Document direct from GCP
                    var storedDocument = await GCPDocumentHelper.GetGCPDocumentAsync(documentURI, gcpToken, CancellationToken.None);

                    // Upsert Document - (Same URI but with a PUT)
                    var response = await _documentUpsertProcessor.UpsertDocumentAsync(documentURI, gcpToken, storedDocument, CancellationToken.None);

                    if (response)
                    {
                        return Ok();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FAILED trying to Publish Report Storage Notification", iaocCode, reportPeriod);
            }

            return BadRequest();
        }
    }
}