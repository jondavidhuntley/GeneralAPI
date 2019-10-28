using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralAPI.Services
{
    /// <summary>
    /// Deletes Document from GCP using GCP API
    /// </summary>
    public class DocumentDeletionProcessor : IDocumentDeletionProcessor
    {
        private readonly string _documentAPIEndPoint;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<DocumentDeletionProcessor> _logger;

        /// <summary>
        /// Document Deletion Processor
        /// </summary>
        /// <param name="documentEndPoint">Document API End Point</param>
        /// <param name="logger">Logger</param>
        public DocumentDeletionProcessor(string documentEndPoint, ILogger<DocumentDeletionProcessor> logger)
        {
            _documentAPIEndPoint = documentEndPoint;
            _logger = logger;
        }

        /// <summary>
        /// Delete Document from Document Service
        /// </summary>
        /// <param name="schemaId">GCP Schema Id</param>
        /// <param name="documentId">GCP Document ID</param>
        /// <param name="clientToken">Client Token</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Http Response Message</returns>
        public async Task<bool> DeleteDocumentAsync(string schemaId, string documentId, string clientToken, CancellationToken cancellationToken)
        {
            bool retVal = false;
            var path = "/" + schemaId + "/" + documentId;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);
                using (var request = new HttpRequestMessage(HttpMethod.Delete, _documentAPIEndPoint + path))
                {
                    var task = client.SendAsync(request, cancellationToken);
                    using (var response = await task)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            retVal = true;
                        }
                    }
                }
            }

            return retVal;
        }
    }
}