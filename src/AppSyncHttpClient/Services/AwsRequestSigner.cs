using Amazon.Runtime;
using AppSyncHttpClient.Extensions;
using AppSyncHttpClient.Utilities;
using System.Security.Cryptography;

namespace AppSyncHttpClient.Services
{
    internal class AwsRequestSigner
    {
        private readonly AWSCredentials _credentials;
        private readonly string _region;

        public AwsRequestSigner(AWSCredentials credentials, string region)
        {
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _region = region ?? throw new ArgumentNullException(nameof(region));
        }

        internal async Task SignAsync(HttpRequestMessage request)
        {
            var uri = request.RequestUri ?? throw new InvalidOperationException("Request URI is missing.");
            var credentials = await _credentials.GetCredentialsAsync();

            var now = DateTime.UtcNow;
            var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'");
            var dateStamp = now.ToString("yyyyMMdd");

            request.AddHostHeader(uri);
            request.AddAmzDateHeader(amzDate);

            if (!string.IsNullOrEmpty(credentials.Token))
                request.Headers.TryAddWithoutValidation("x-amz-security-token", credentials.Token);

            byte[] bodyBytes = request.Content == null ? Array.Empty<byte>() : await request.Content.ReadAsByteArrayAsync();
            string payloadHash = HexConverter.ToHex(SHA256.HashData(bodyBytes));

            request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);

            request.AddAuthorization(uri, dateStamp, _region, amzDate, credentials, payloadHash);
        }
    }
}
