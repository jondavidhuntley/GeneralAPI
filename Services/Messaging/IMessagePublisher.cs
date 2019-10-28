using System.Threading.Tasks;

namespace GeneralAPI.Services.Messaging
{
    /// <summary>
    /// IMessage Publisher Interface
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Identify if for GCP Usage - Used Key Vault Connection String to Service Bus
        /// </summary>
        bool IsGCPVersion { get; }

        /// <summary>
        /// Publish Topical Notification to Service Bus
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <param name="messageJson">Message Payload JSON</param>
        /// <returns>Boolean indicating Success</returns>
        Task<bool> PublishNotificationAsync(string topic, string messageJson);
    }
}