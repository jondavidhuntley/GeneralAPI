using GeneralAPI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneralAPI.Services.DataServices
{
    /// <summary>
    /// Document Service Report DataService
    /// </summary>
    public interface IReportDataService
    {
        /// <summary>
        /// Register &amp; Store New Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>Command Response</returns>
        CommandResponse RegisterNewReport(Report report);

        /// <summary>
        /// Delete Existing Report
        /// </summary>
        /// <param name="documentId">GCP Report Document ID</param>
        /// <returns>Command Response</returns>
        CommandResponse DeleteReport(string documentId);

        /// <summary>
        /// Get Report Detail
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Report Year</param>
        /// <returns>Report</returns>
        Report GetReportDetail(string iaocCode, ReportPeriod reportPeriod, string reportType, int year);

        /// <summary>
        /// Get List of Document Ids for Deletion
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Report Year</param>
        /// <returns>List of Document ID</returns>
        IEnumerable<Guid> GetReportDocumentsForDeletions(string iaocCode, ReportPeriod reportPeriod, string reportType, int year);

        /// <summary>
        /// Test if ALL Core Reports are Available for a Given Airline, Reporting Period and Year
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Annual, Qn, Hn</param>
        /// <param name="year">Report Year</param>
        /// <returns>true or false</returns>
        Task<bool> TestAllCoreReportsAvailableAsync(string iaocCode, string reportPeriod, int year);
    }
}