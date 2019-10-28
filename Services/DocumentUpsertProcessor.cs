using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralAPI.Services
{
    /// <summary>
    /// Handles Document Updates to GCP - Needs porting to GCP SDK
    /// </summary>
    public class DocumentUpsertProcessor : IDocumentUpsertProcessor
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<DocumentUpsertProcessor> _logger;

        /// <summary>
        /// Document Upsert Processor
        /// </summary>
        /// <param name="logger">Logger</param>
        public DocumentUpsertProcessor(ILogger<DocumentUpsertProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Post New Document to GCP
        /// </summary>
        /// <param name="targetUri">Target URL</param>
        /// <param name="clientToken">Client Token</param>
        /// <param name="payload">JSON Payload</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Success Indicator</returns>
        public async Task<bool> UpsertDocumentAsync(string targetUri, string clientToken, string payload, CancellationToken cancellationToken)
        {
            bool success = false;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

                using (var request = new HttpRequestMessage(HttpMethod.Put, targetUri))
                {
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                    using (var response = await client.SendAsync(request, cancellationToken))
                    {
                        success = response.IsSuccessStatusCode;
                    }
                }
            }

            return success;
        }
    }
}