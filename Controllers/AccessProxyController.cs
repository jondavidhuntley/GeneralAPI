using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralAPI.Controllers
{
    /// <summary>
    /// Document Service Access Proxy Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccessProxyController : Controller
    {
        private const string CONTENT_TYPE = "application/json";
        private readonly string _taaDocumentAPIAudience;
        private readonly string _gcpTokenServiceUrl;
        private readonly string _grantType;
        private readonly ILogger<AccessProxyController> _logger;

        /// <summary>
        /// Access Proxy Controller
        /// </summary>
        /// <param name="configuration">Configuration Interface</param>
        /// <param name="logger">Event Logger</param>
        public AccessProxyController(IConfiguration configuration, ILogger<AccessProxyController> logger)
        {
            _taaDocumentAPIAudience = configuration.GetValue<string>("TaaDocumentAPIAudience");
            _gcpTokenServiceUrl = configuration.GetValue<string>("GcpTokenServiceUrl");
            _grantType = configuration.GetValue<string>("GrantType");

            _logger = logger;
        }

        /// <summary>
        /// Get Auth0 Token for DocumentAPI
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="clientSecret">Client Secret</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        [Route("{clientId}/{clientSecret}")]
        [SwaggerOperation(Summary = "Recovers Document API Auth0 Token")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAccessTokenAsync(string clientId, string clientSecret)
        {
            string token = string.Empty;

            try
            {
                var clientTokenRequest = new ClientTokenRequest
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Audience = _taaDocumentAPIAudience,
                    GrantType = _grantType
                };

                var response = await GetClientTokenAsync(clientTokenRequest, CancellationToken.None);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to fetch Access Token -> Error Message:{ex.Message}");
            }

            return BadRequest("Client Data Not Recognised!");
        }

        /// <summary>
        /// Get Auth0 Token
        /// </summary>
        /// <param name="clientTokenRequest">Client Token Request</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Client Token Response</returns>
        private async Task<ClientTokenResponse> GetClientTokenAsync(ClientTokenRequest clientTokenRequest, CancellationToken cancellationToken)
        {
            var requestJson = JsonConvert.SerializeObject(clientTokenRequest);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(CONTENT_TYPE));

                using (var request = new HttpRequestMessage(HttpMethod.Post, _gcpTokenServiceUrl))
                {
                    request.Content = new StringContent(requestJson, Encoding.UTF8, CONTENT_TYPE);

                    using (var response = await client.SendAsync(request, cancellationToken))
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode == false)
                        {
                            throw new Exception(content + "Status Code :" + response.StatusCode.ToString());
                        }

                        return JsonConvert.DeserializeObject<ClientTokenResponse>(content);
                    }
                }
            }
        }
    }
}