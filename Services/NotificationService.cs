using GeneralAPI.Services.DataServices;
using GeneralAPI.Services.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GeneralAPI.Services
{
    /// <summary>
    /// Handles Topical Notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IReportDataService _reportDataService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<NotificationService> _logger;
        private readonly string _secondaryReportNotificationTopic;

        /// <summary>
        /// Creates new instance of Notification Service
        /// </summary>
        /// <param name="reportDataService">Report Data Service</param>
        /// <param name="messagePublisher">Service Bus Message Publisher</param>
        /// <param name="secondaryReportNotificationTopic">Topic Name</param>
        /// <param name="logger">Event Logger</param>
        public NotificationService(IReportDataService reportDataService, IMessagePublisher messagePublisher, string secondaryReportNotificationTopic, ILogger<NotificationService> logger)
        {
            _reportDataService = reportDataService;
            _messagePublisher = messagePublisher;
            _secondaryReportNotificationTopic = secondaryReportNotificationTopic;
            _logger = logger;
        }

        /// <summary>
        /// Publish Secondary Report Notification when All Primary reports are available
        /// </summary>
        /// <param name="airlineCode">Airline ICAO Code</param>
        /// <param name="reportPeriod">Annual, Qn, Hn</param>
        /// <param name="currency">Currency</param>
        /// <param name="year">Year</param>
        /// <returns>Notification Response</returns>
        public async Task<NotificationActionResponse> TestAndPublishSecondaryReportNotificationAsync(string airlineCode, string reportPeriod, string currency, int year)
        {
            var response = new NotificationActionResponse
            {
                Topic = _secondaryReportNotificationTopic
            };

            try
            {
                var allCoreReportsAvailable = await _reportDataService.TestAllCoreReportsAvailableAsync(airlineCode, reportPeriod, year);

                if (!allCoreReportsAvailable)
                {
                    var message = "Start Secondary Report Generation!";

                    var notification = new ReportNotification
                    {
                        AirlineICAOCode = airlineCode,
                        ReportingPeriod = reportPeriod,
                        Currency = currency,
                        Message = message
                    };

                    var payload = Newtonsoft.Json.JsonConvert.SerializeObject(notification);

                    response.MessagePublished = await _messagePublisher.PublishNotificationAsync(_secondaryReportNotificationTopic, payload);
                }
            }
            catch (Exception ex)
            {
                response.FaultMessage = ex.Message;
                _logger.LogCritical(ex, airlineCode, reportPeriod, currency, year);
            }

            return response;
        }
    }
}