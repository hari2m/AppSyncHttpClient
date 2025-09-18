using Amazon.Runtime;
using AppSyncHttpClient.Extensions;
using AppSyncHttpClient.Models;
using AppSyncHttpClient.Utilities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AppSyncHttpClient.Services
{
    public class AppSyncPublishAction
    {
        private readonly ILogger<AppSyncPublishAction> _logger;
        private readonly AWSCredentials _awsCreds;
        private readonly HttpClient _httpClient;
        private readonly string _region;
        private readonly AwsRequestSigner _signer;

        public AppSyncPublishAction(ILogger<AppSyncPublishAction> logger, AWSCredentials awsCreds, HttpClient httpClient, string region)
        {
            _logger = logger;
            _awsCreds = awsCreds ?? throw new ArgumentNullException(nameof(awsCreds));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _region = region ?? throw new ArgumentNullException(nameof(region));
            if (_httpClient.BaseAddress == null)
                throw new InvalidOperationException("Http client base address must be configured to proceed!");

            _signer = new AwsRequestSigner(_awsCreds, _region);
        }

        public async Task ExecuteAsync<T>(string topic, T message)
        {
            var payload = new PublishPayload<string>
            {
                channel = topic,
                events = [JsonSerializer.Serialize(message)]
            };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var request = CreateHttpRequest(jsonPayload);

            await _signer.SignAsync(request);

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Publish failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {responseBody}");

            if (response.Headers.TryGetValues("x-amzn-RequestId", out var requestIds))
                _logger.LogInformation($"AppSync publish succeeded. RequestId={requestIds.FirstOrDefault()}");
        }

        private HttpRequestMessage CreateHttpRequest(string jsonPayload)
        {
            var uri = new Uri(_httpClient.BaseAddress!, "event");

            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            request.Content.Headers.TryAddWithoutValidation("content-encoding", "amz-1.0");

            return request;
        }

        private async Task SignRequestAsync(HttpRequestMessage request)
        {
            var uri = request.RequestUri ?? throw new InvalidOperationException("Request Uri missing.");
            if (!uri.IsAbsoluteUri) throw new InvalidOperationException("Request uri must be absolute before signing.");
            var credentials = await _awsCreds.GetCredentialsAsync();

            var now = DateTime.UtcNow;
            var amzDate = now.ToString("yyyyMMdd'T'HHmmss'z'");

            request.AddHostHeader(uri);
            request.AddAmzDateHeader(amzDate);

            if (!string.IsNullOrEmpty(credentials.Token))
                request.Headers.TryAddWithoutValidation("x-amz-security-token", credentials.Token);

            byte[] bodyBytes = request.Content == null ? Array.Empty<byte>() : await request.Content.ReadAsByteArrayAsync();
            var payloadHash = HexConverter.ToHex(SHA256.HashData(bodyBytes));
            var body = request.Content == null ? "" : Encoding.UTF8.GetString(bodyBytes);
            request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);

            request.AddAuthorization(uri, now.ToString("yyyyMMdd"), _region, amzDate, credentials, payloadHash);
        }
    }
}
