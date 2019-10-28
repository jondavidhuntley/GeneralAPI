using GeneralAPI.Services.DataServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralAPI.Services
{
    /// <summary>
    /// Deletes multiple historic reports from both GCP &amp; Document Service DB
    /// </summary>
    public class HistoricDataDeletionProcessor : IHistoricDataDeletionProcessor
    {
        private readonly IReportDataService _reportDataService;
        private readonly IClientTokenHandler _clientTokenHandler;
        private readonly IDocumentDeletionProcessor _documentDeletionProcessor;
        private readonly ILogger<HistoricDataDeletionProcessor> _logger;

        /// <summary>
        /// Historic Data Deletion Processor
        /// </summary>
        /// <param name="reportDataService">Report Data Service</param>
        /// <param name="clientTokenHandler">GCP Client Token Hanlder</param>
        /// <param name="documentDeletionProcessor">Document Deletion Processor</param>
        /// <param name="logger">Event Logger</param>
        public HistoricDataDeletionProcessor(IReportDataService reportDataService, IClientTokenHandler clientTokenHandler, IDocumentDeletionProcessor documentDeletionProcessor, ILogger<HistoricDataDeletionProcessor> logger)
        {
            _reportDataService = reportDataService;
            _clientTokenHandler = clientTokenHandler;
            _documentDeletionProcessor = documentDeletionProcessor;
            _logger = logger;
        }

        /// <summary>
        /// Delete Historic Reports
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Annual, Q1, Q2</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Report Year</param>
        /// <returns>Command Response</returns>
        public async Task<CommandResponse> DeleteHistoricReportsAsync(string iaocCode, ReportPeriod reportPeriod, string reportType, int year)
        {
            CommandResponse response = new CommandResponse
            {
                Information = $"Deleting Historic Reports for:{iaocCode} period:{reportPeriod} type:{reportType} year:{year}",
                Success = true
            };
            try
            {
                var schemaList = GetAssociatedSchemaList(reportType.ToLowerInvariant());

                if (schemaList != null && schemaList.Count > 0)
                {
                    foreach (string schemaId in schemaList)
                    {
                        _logger.LogInformation($"Deleting Historic Records for :{iaocCode} period:{reportPeriod} type:{schemaId} year:{year}");
                        var schemaDeleteResponse = await DeleteHistoricRecordsAsync(iaocCode, reportPeriod, schemaId, year);
                        if (schemaDeleteResponse.Success)
                        {
                            _logger.LogInformation($"Successfully Deleted Historic Records for :{iaocCode} period:{reportPeriod} type:{schemaId} year:{year}");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to Delete Historic Records for :{iaocCode} period:{reportPeriod} type:{schemaId} year:{year}");
                            _logger.LogError(schemaDeleteResponse.FaultMessage);
                            response.Success = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                response.FaultMessage = ex.Message;
                response.Success = false;
            }

            return response;
        }

        /// <summary>
        /// Delete Historic Records for Specified Airline, Reporting Period, ReportType (GCP SchemaId) and Year
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period - Annual, Q1, Q2</param>
        /// <param name="schemaId">GCP SchemaId e.g. raw-income-statement</param>
        /// <param name="year">Report Year</param>
        /// <returns>Command Response</returns>
        public async Task<CommandResponse> DeleteHistoricRecordsAsync(string iaocCode, ReportPeriod reportPeriod, string schemaId, int year)
        {
            CommandResponse response = new CommandResponse
            {
                Information = $"Deleting Historic Reports for:{iaocCode} period:{reportPeriod} type:{schemaId} year:{year}",
                Success = true
            };
            try
            {
                var deleteList = _reportDataService.GetReportDocumentsForDeletions(iaocCode, reportPeriod, schemaId, year);
                if (deleteList != null)
                {
                    var token = _clientTokenHandler.GetValidToken();
                    foreach (Guid documentId in deleteList)
                    {
                        // DELETE Document from GCP
                        var gcpDeletionResponse = await _documentDeletionProcessor.DeleteDocumentAsync(schemaId, documentId.ToString(), token, CancellationToken.None);
                        if (gcpDeletionResponse == true)
                        {
                            // Delete Report in Document Service Database
                            var documentAPIResponse = _reportDataService.DeleteReport(documentId.ToString());
                            if (documentAPIResponse != null && !documentAPIResponse.Success)
                            {
                                response.Success = false;
                            }
                        }
                        else
                        {
                            response.Success = false;
                            _logger.LogWarning($"FAILED to DELETE Document from GCP for:{iaocCode} period:{reportPeriod} type:{schemaId} year:{year}");
                        }
                    }

                    if (!response.Success)
                    {
                        _logger.LogWarning($"FAILED to DELETE ALL Historic Documents for :{iaocCode} period:{reportPeriod} type:{schemaId} year:{year}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                response.FaultMessage = ex.Message;
                response.Success = false;
            }

            return response;
        }

        /// <summary>
        /// Get List of Schemas for Deletion based on Report Type
        /// </summary>
        /// <param name="reportType">Completed Report Type</param>
        /// <returns>List of Report Schema to Delete</returns>
        public List<string> GetAssociatedSchemaList(string reportType)
        {
            List<string> schemaList = new List<string>();
            switch (reportType.ToLowerInvariant())
            {
                case "input-control":
                    schemaList.Add("raw-input-control-sheet");
                    schemaList.Add("input-control");
                    break;
                case "operational-data":
                    schemaList.Add("raw-operational-data");
                    schemaList.Add("partial-operational-data");
                    schemaList.Add("revenue-operational-data");
                    schemaList.Add("operational-data");
                    break;
                case "income-statement":
                    schemaList.Add("raw-income-statement");
                    schemaList.Add("partial-income-statement");
                    schemaList.Add("percentage-change-income-statement");
                    schemaList.Add("income-statement");
                    break;
                case "balance-sheet":
                    schemaList.Add("raw-balance-sheet");
                    schemaList.Add("balance-sheet");
                    break;
            }

            return schemaList;
        }
    }
}