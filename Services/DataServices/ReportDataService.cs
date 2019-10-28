using GeneralAPI.Model;
using GeneralAPI.Services.DataServices.DAL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneralAPI.Services.DataServices
{
    /// <summary>
    /// Document Report Data Service
    /// </summary>
    public class ReportDataService : IReportDataService
    {
        private readonly IReportDAL _reportDAL;
        private readonly ILogger<ReportDataService> _logger;

        /// <summary>
        /// Creates new instance of Report Data Service
        /// </summary>
        /// <param name="reportDAL">Report DAL</param>
        /// <param name="logger">Event Logger</param>
        public ReportDataService(IReportDAL reportDAL, ILogger<ReportDataService> logger)
        {
            _reportDAL = reportDAL;
            _logger = logger;
        }

        /// <summary>
        /// Register &amp; Store New Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>Command Response</returns>
        public CommandResponse RegisterNewReport(Report report)
        {
            var response = new CommandResponse();
            _logger.LogInformation($"Creating New Report for Airline IAOC Code: {report.Airline}");
            try
            {
                var newRecordId = _reportDAL.CreateNewReportAsync(report).GetAwaiter().GetResult();
                if (newRecordId > 0)
                {
                    response.RecordId = newRecordId;
                    response.Success = true;
                    response.Information = $"Successfully Created New Report Record for:{report.ReportType} with ID:{newRecordId}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Unable to Create New Airline Report Document: {report.ReportType} - Exception:{ex.Message}-{ex.StackTrace}");
                response.FaultMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Delete Existing Report
        /// </summary>
        /// <param name="documentId">GCP Report Document ID</param>
        /// <returns>Command Response</returns>
        public CommandResponse DeleteReport(string documentId)
        {
            var response = new CommandResponse();
            _logger.LogInformation($"Deleting Report with DocumentId: {documentId}");
            try
            {
                var affectedRows = _reportDAL.DeleteReportAsync(documentId).GetAwaiter().GetResult();
                if (affectedRows > 0)
                {
                    response.Success = true;
                    response.Information = $"Successfully Deleted Report Record with ID:{documentId}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Unable to Delete Report Document: {documentId} - Exception:{ex.Message}-{ex.StackTrace}");
                response.FaultMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Get Report Detail
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Report Year</param>
        /// <returns>Report</returns>
        public Report GetReportDetail(string iaocCode, ReportPeriod reportPeriod, string reportType, int year)
        {
            Report report = null;
            // Fetch Matching Reports from Data Base
            var data = _reportDAL.GetReportsAsync(iaocCode, reportPeriod, reportType, year).GetAwaiter().GetResult();
            if (data != null)
            {
                // Create List (returned in Created Date Order DESC)
                var reports = data.ToList();
                // Return First Entry
                if (reports != null && reports.Count > 0)
                {
                    report = reports[0];
                }
            }

            return report;
        }

        /// <summary>
        /// Test if ALL Core Reports are Available for a Given Airline, Reporting Period and Year
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Annual, Qn, Hn</param>
        /// <param name="year">Report Year</param>
        /// <returns>true or false</returns>
        public async Task<bool> TestAllCoreReportsAvailableAsync(string iaocCode, string reportPeriod, int year)
        {
            bool available = false;

            var coreReportTypes = new string[] { "input-control", "operational-data", "income-statement", "balance-sheet", "cash-flow-statement", "share-info" };

            try
            {
                var keyData = await _reportDAL.GetCoreReportKeysAsync(iaocCode, reportPeriod, year);

                if (keyData != null)
                {
                    var availableReports = keyData.ToList();
                    var allAvailable = true;

                    foreach (string type in coreReportTypes)
                    {
                        var reportAvailable = false;

                        foreach (ReportKey report in availableReports)
                        {
                            if (report.ReportType == type)
                            {
                                reportAvailable = true;
                                break;
                            }
                        }

                        if (!reportAvailable)
                        {
                            allAvailable = false;
                            break;
                        }
                    }

                    available = allAvailable;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, iaocCode, reportPeriod, year);
            }

            return available;
        }

        /// <summary>
        /// Get List of Document Ids for Deletion
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Report Year</param>
        /// <returns>List of Document ID</returns>
        public IEnumerable<Guid> GetReportDocumentsForDeletions(string iaocCode, ReportPeriod reportPeriod, string reportType, int year)
        {
            return _reportDAL.GetReportDocumentIdsForDeletionAsync(iaocCode, reportPeriod, reportType, year).GetAwaiter().GetResult();
        }
    }
}