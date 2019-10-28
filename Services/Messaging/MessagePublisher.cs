using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace GeneralAPI.Services.Messaging
{
    /// <summary>
    /// Publishes Messages to Service Bus
    /// </summary>
    public class MessagePublisher : IMessagePublisher
    {
        private readonly string _connectionString;
        private readonly ILogger<MessagePublisher> _logger;

        private ServiceBusConnectionStringBuilder _connectionBuilder;
        private TokenProvider _tokenProvider;

        /// <summary>
        /// Message Publisher Service
        /// </summary>
        /// <param name="connectionString">Service Bus Connection</param>
        /// <param name="isGCP">Is for GCP Use</param>
        /// <param name="logger">Event Logger</param>
        public MessagePublisher(string connectionString, bool isGCP, ILogger<MessagePublisher> logger)
        {
            _connectionString = connectionString;
            _logger = logger;

            IsGCPVersion = isGCP;

            _connectionBuilder = new ServiceBusConnectionStringBuilder(connectionString);
            _tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                _connectionBuilder.SasKeyName,
                _connectionBuilder.SasKey);
        }

        /// <summary>
        /// Identify if for GCP Usage - Used Key Vault Connection String to Service Bus
        /// </summary>
        public bool IsGCPVersion { get; private set; }

        /// <summary>
        /// Publish Topical Notification to Service Bus
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <param name="messageJson">Message Payload JSON</param>
        /// <returns>Boolean indicating Success</returns>
        public async Task<bool> PublishNotificationAsync(string topic, string messageJson)
        {
            bool success = false;

            try
            {
                if (string.IsNullOrEmpty(topic))
                {
                    _logger.LogWarning("Topic is Empty!");
                    return false;
                }

                if (string.IsNullOrEmpty(messageJson))
                {
                    _logger.LogWarning("Notification Message Payload is Empty!");
                    return false;
                }

                // Prepare Message
                var message = new Message(Encoding.UTF8.GetBytes(messageJson));

                _logger.LogInformation($"Publishing Topic Message: {messageJson}");

                // Create Message Sender
                var sender = new MessageSender(
                    _connectionBuilder.Endpoint,
                    topic,
                    _tokenProvider,
                    TransportType.Amqp);

                await sender.SendAsync(message);

                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, topic, messageJson);
            }

            return success;
        }
    }
}