using System.Threading;
using System.Threading.Tasks;

namespace GeneralAPI.Services
{
    /// <summary>
    /// Document Deletion Processor Interface
    /// </summary>
    public interface IDocumentDeletionProcessor
    {
        /// <summary>
        /// Delete Document from Document Service
        /// </summary>
        /// <param name="schemaId">GCP Schema Id</param>
        /// <param name="documentId">GCP Document ID</param>
        /// <param name="clientToken">Client Token</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Http Response Message</returns>
        Task<bool> DeleteDocumentAsync(string schemaId, string documentId, string clientToken, CancellationToken cancellationToken);
    }
}