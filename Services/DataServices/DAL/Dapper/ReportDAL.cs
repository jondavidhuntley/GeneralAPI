using Dapper;
using GeneralAPI.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace GeneralAPI.Services.DataServices.DAL.Dapper
{
    /// <summary>
    /// Document Report DAL
    /// </summary>
    public class ReportDAL : IReportDAL
    {
        private readonly string _sqlConnection;
        private readonly ILogger<ReportDAL> _logger;

        /// <summary>
        /// Creates new instance of ReportDAL
        /// </summary>
        /// <param name="sqlConnection">SQL Server Database Connection</param>
        /// <param name="logger">Event Logger</param>
        public ReportDAL(string sqlConnection, ILogger<ReportDAL> logger)
        {
            _sqlConnection = sqlConnection;
            _logger = logger;
        }

        /// <summary>
        /// Create / Store new Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>Report Id</returns>
        public async Task<int> CreateNewReportAsync(Report report)
        {
            int newReportId = 0;
            using (var sqlConnection = new SqlConnection(_sqlConnection))
            {
                await sqlConnection.OpenAsync();
                var dynParams = new DynamicParameters();
                dynParams.Add("@AirlineCode", report.Airline, DbType.String);
                dynParams.Add("@ReportPeriod", report.Period.ToString(), DbType.String);
                dynParams.Add("@ReportType", report.ReportType, DbType.String);
                dynParams.Add("@DocumentId", report.DocumentId, DbType.Guid);
                dynParams.Add("@Currency", report.Currency, DbType.String);
                dynParams.Add("@Rate", report.ExchangeRate, DbType.Decimal);
                dynParams.Add("@SpotRate", report.IsSpotRate, DbType.Boolean);
                dynParams.Add("@ReportDate", report.ReportDate, DbType.Date);
                dynParams.Add("@ReportId", null, DbType.Int32, ParameterDirection.Output);
                var affectedRows = await sqlConnection.ExecuteAsync("usp_Report_Insert", dynParams, null, 30, CommandType.StoredProcedure);
                if (affectedRows > 0)
                {
                    newReportId = dynParams.Get<int>("ReportId");
                }
            }

            return newReportId;
        }

        /// <summary>
        /// Delete Existing Report
        /// </summary>
        /// <param name="documentId">Report DocumentId</param>
        /// <returns>Affected Rows</returns>
        public async Task<int> DeleteReportAsync(string documentId)
        {
            int affectedRows = 0;
            using (var sqlConnection = new SqlConnection(_sqlConnection))
            {
                await sqlConnection.OpenAsync();
                var dynParams = new DynamicParameters();
                dynParams.Add("@DocumentId", new Guid(documentId), DbType.Guid);
                affectedRows = await sqlConnection.ExecuteAsync("usp_Report_Delete", dynParams, null, 30, CommandType.StoredProcedure);
            }

            return affectedRows;
        }

        /// <summary>
        /// Get Reports
        /// </summary>
        /// <param name="iaocCode">Airline IAOC Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="reportType">Report Type/Schema</param>
        /// <param name="year">Year</param>
        /// <returns>List of Report</returns>
        public async Task<IEnumerable<Report>> GetReportsAsync(string iaocCode, ReportPeriod reportPeriod, string reportType, int year)
        {
            IEnumerable<Report> data = null;
            _logger.LogInformation("Starting Report Data Fetch");
            try
            {
                if (string.IsNullOrEmpty(_sqlConnection))
                {
                    throw new Exception("No SQL Server Connection String");
                }

                using (var sqlConnection = new SqlConnection(_sqlConnection))
                {
                    await sqlConnection.OpenAsync();
                    var dynParams = new DynamicParameters();
                    dynParams.Add("@AirlineCode", iaocCode, DbType.String);
                    dynParams.Add("@ReportPeriod", reportPeriod.ToString(), DbType.String);
                    dynParams.Add("@ReportType", reportType, DbType.String);
                    dynParams.Add("@Year", year, DbType.Int32);
                    var results = await sqlConnection.QueryMultipleAsync("usp_Report_Select", dynParams, commandType: CommandType.StoredProcedure);
                    data = await results.ReadAsync<Report>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            _logger.LogInformation("Completed Data Fetch");
            return data;
        }

        /// <summary>
        /// Get Report Document IDs for Deletion
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="year">Year</param>
        /// <returns>List of Document IDs</returns>
        public async Task<IEnumerable<Guid>> GetReportDocumentIdsForDeletionAsync(string iaocCode, ReportPeriod reportPeriod, string reportType, int year)
        {
            IEnumerable<Guid> data = null;
            _logger.LogInformation("Starting Historic Report Data DocumentID Fetch");
            try
            {
                if (string.IsNullOrEmpty(_sqlConnection))
                {
                    throw new Exception("No SQL Server Connection String");
                }

                using (var sqlConnection = new SqlConnection(_sqlConnection))
                {
                    await sqlConnection.OpenAsync();
                    var dynParams = new DynamicParameters();
                    dynParams.Add("@AirlineId", iaocCode, DbType.String);
                    dynParams.Add("@ReportPeriod", reportPeriod.ToString(), DbType.String);
                    dynParams.Add("@ReportType", reportType, DbType.String);
                    dynParams.Add("@Year", year, DbType.Int32);
                    var results = await sqlConnection.QueryMultipleAsync("usp_Report_Select_Documents_For_Delete", dynParams, commandType: CommandType.StoredProcedure);
                    data = await results.ReadAsync<Guid>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            _logger.LogInformation("Completed Data Fetch");
            return data;
        }

        /// <summary>
        /// Get Available Core Report Keys
        /// </summary>
        /// <param name="iaocCode">Airline Code</param>
        /// <param name="reportPeriod">Report Period</param>
        /// <param name="year">Year</param>
        /// <returns>List of ReportKey</returns>
        public async Task<IEnumerable<ReportKey>> GetCoreReportKeysAsync(string iaocCode, string reportPeriod, int year)
        {
            IEnumerable<ReportKey> data = null;
            _logger.LogInformation("Starting Core Report Data Fetch");
            try
            {
                if (string.IsNullOrEmpty(_sqlConnection))
                {
                    throw new Exception("No SQL Server Connection String");
                }

                using (var sqlConnection = new SqlConnection(_sqlConnection))
                {
                    await sqlConnection.OpenAsync();
                    var dynParams = new DynamicParameters();
                    dynParams.Add("@airlineICAO", iaocCode, DbType.String);
                    dynParams.Add("@reportPeriod", reportPeriod, DbType.String);
                    dynParams.Add("@year", year, DbType.Int32);
                    var results = await sqlConnection.QueryMultipleAsync("usp_Select_DocumentIds_With_Stored_Core_Reports", dynParams, commandType: CommandType.StoredProcedure);
                    data = await results.ReadAsync<ReportKey>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            _logger.LogInformation("Completed Data Fetch");

            return data;
        }
    }
}