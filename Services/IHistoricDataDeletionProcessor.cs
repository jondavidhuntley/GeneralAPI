using System.Threading.Tasks;

namespace GeneralAPI.Services
{
    /// <summary>
    /// Historic Data Deletion Processor
    /// </summary>
    public interface IHistoricDataDeletionProcessor
    {
        /// <summary>
        /// Delete Historic Reports
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Annual, Q1, Q2</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Report Year</param>
        /// <returns>Command Response</returns>
        Task<CommandResponse> DeleteHistoricReportsAsync(string iaocCode, ReportPeriod reportPeriod, string reportType, int year);

        /// <summary>
        /// Delete Historic Records for Specified Airline, Reporting Period, ReportType (GCP SchemaId) and Year
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period - Annual, Q1, Q2</param>
        /// <param name="schemaId">GCP SchemaId e.g. raw-income-statement</param>
        /// <param name="year">Report Year</param>
        /// <returns>Command Response</returns>
        Task<CommandResponse> DeleteHistoricRecordsAsync(string iaocCode, ReportPeriod reportPeriod, string schemaId, int year);
    }
}