using System.Threading.Tasks;

namespace GeneralAPI.Services
{
    /// <summary>
    /// Notification Service Interface
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Publish Secondary Report Notification when All Primary reports are available
        /// </summary>
        /// <param name="airlineCode">Airline ICAO Code</param>
        /// <param name="reportPeriod">Annual, Qn, Hn</param>
        /// <param name="currency">Currency</param>
        /// <param name="year">Year</param>
        /// <returns>Notification Response</returns>
        Task<NotificationActionResponse> TestAndPublishSecondaryReportNotificationAsync(string airlineCode, string reportPeriod, string currency, int year);
    }
}