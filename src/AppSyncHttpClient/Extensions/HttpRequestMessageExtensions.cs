using Amazon.Runtime;
using System.Security.Cryptography;
using System.Text;
using AppSyncHttpClient.Utilities;
using AppSyncHttpClient.Constants;

namespace AppSyncHttpClient.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static void AddAuthorization(this HttpRequestMessage request, Uri uri, string date, string region, string amzDate, ImmutableCredentials credentials, string payloadHash)
        {
            string scope = AwsSigningFormatter.FormatScope(date, region);

            var headersToSign = GetHeadersToSign(credentials);

            var canonicalHeaders = string.Join("\n", headersToSign.Select(h => $"{h}:{HttpHeaderHelper.GetHeader(request, h).Trim()}")) + "\n";
            var signedHeaders = string.Join(";", headersToSign);

            var canonicalRequest = AwsSigningFormatter.FormatCanonicalRequest(request.Method, uri.AbsolutePath, canonicalHeaders, signedHeaders, payloadHash);
            var canonicalRequestHash = HexConverter.ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)));

            var stringToSign = AwsSigningFormatter.FormatStringToSign(amzDate, scope, canonicalRequestHash);
            var signature = GenerateSignature(credentials, region, date, AwsRequestSigningConstants.Service, stringToSign);

            var authorizationHeader = AwsSigningFormatter.FormatAuthorizationHeader(credentials.AccessKey, scope, signedHeaders, signature);
            request.Headers.TryAddWithoutValidation(AwsRequestSigningConstants.Headers.Authorization, authorizationHeader);
        }

        private static string[] GetHeadersToSign(ImmutableCredentials credentials)
        {
            var headers = new[]
            {
                AwsRequestSigningConstants.Headers.ContentEncoding,
                AwsRequestSigningConstants.Headers.ContentType,
                AwsRequestSigningConstants.Headers.Host,
                AwsRequestSigningConstants.Headers.AmzContentSha256,
                AwsRequestSigningConstants.Headers.AmzDate,
                string.IsNullOrEmpty(credentials.Token) ? null : AwsRequestSigningConstants.Headers.AmzSecurityToken
            };

            return headers.Where(h => h != null).Select(h => h!).OrderBy(h => h).ToArray();
        }

        public static void AddHostHeader(this HttpRequestMessage request, Uri uri)
        {
            if (!request.Headers.Contains("host"))
                request.Headers.TryAddWithoutValidation("host", uri.Host);
        }

        public static void AddAmzDateHeader(this HttpRequestMessage request, string amzDate)
        {
            request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
        }

        private static string GenerateSignature(ImmutableCredentials credentials, string region, string date, string service, string stringToSign)
        {
            byte[] kDate = Hmac($"AWS4{credentials.SecretKey}", date);
            byte[] kRegion = ComputeHmacSHA256(kDate, region);
            byte[] kService = ComputeHmacSHA256(kRegion, service);
            byte[] kSigning = ComputeHmacSHA256(kService, "aws4_request");

            return HexConverter.ToHex(ComputeHmacSHA256(kSigning, stringToSign));
        }

        private static byte[] Hmac(string key, string data) =>
            ComputeHmacSHA256(Encoding.UTF8.GetBytes(key), data);

        private static byte[] ComputeHmacSHA256(byte[] key, string data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}