using GeneralAPI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneralAPI.Services.DataServices.DAL
{
    /// <summary>
    /// Document Service Report DAL
    /// </summary>
    public interface IReportDAL
    {
        /// <summary>
        /// Create / Store new Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>Report Id</returns>
        Task<int> CreateNewReportAsync(Report report);

        /// <summary>
        /// Delete Existing Report
        /// </summary>
        /// <param name="documentId">Report DocumentId</param>
        /// <returns>Affected Rows</returns>
        Task<int> DeleteReportAsync(string documentId);

        /// <summary>
        /// Get Reports
        /// </summary>
        /// <param name="iaocCode">Airline IAOC Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="reportType">Report Type/Schema</param>
        /// <param name="year">Year</param>
        /// <returns>List of Report</returns>
        Task<IEnumerable<Report>> GetReportsAsync(string iaocCode, ReportPeriod reportPeriod, string reportType, int year);

        /// <summary>
        /// Get Report Document IDs for Deletion
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Year</param>
        /// <returns>List of Document IDs</returns>
        Task<IEnumerable<Guid>> GetReportDocumentIdsForDeletionAsync(string iaocCode, ReportPeriod reportPeriod, string reportType, int year);

        /// <summary>
        /// Get Available Core Report Keys
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="year">Year</param>
        /// <returns>List of ReportKey</returns>
        Task<IEnumerable<ReportKey>> GetCoreReportKeysAsync(string iaocCode, string reportPeriod, int year);
    }
}