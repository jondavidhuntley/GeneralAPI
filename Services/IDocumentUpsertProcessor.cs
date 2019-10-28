using System.Threading;
using System.Threading.Tasks;

namespace GeneralAPI.Services
{
    /// <summary>
    /// Document Upsert Processor Interface
    /// </summary>
    public interface IDocumentUpsertProcessor
    {
        /// <summary>
        /// Post New Document to GCP
        /// </summary>
        /// <param name="targetUri">Target URL</param>
        /// <param name="clientToken">Client Token</param>
        /// <param name="payload">Json Payload</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Document Response</returns>
        Task<bool> UpsertDocumentAsync(string targetUri, string clientToken, string payload, CancellationToken cancellationToken);
    }
}