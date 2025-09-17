using AppSyncHttpClient.Constants;

namespace AppSyncHttpClient.Utilities
{
    internal static class AwsSigningFormatter
    {
        internal static string FormatAuthorizationHeader(string accessKey, string scope, string signedHeaders, string signature)
        {
            return $"{AwsRequestSigningConstants.Algorithm} Credential={accessKey}/{scope}, SignedHeaders={signedHeaders}, Signature={signature}";
        }

        internal static string FormatScope(string date, string region)
        {
            return $"{date}/{region}/{AwsRequestSigningConstants.Service}/{AwsRequestSigningConstants.Aws4Request}";
        }

        internal static string FormatCanonicalRequest(HttpMethod method, string path, string canonicalHeaders, string signedHeaders, string payloadHash)
        {
            return $"{method.Method}\n{path}\n\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";
        }

        internal static string FormatStringToSign(string amzDate, string scope, string canonicalRequestHash)
        {
            return $"{AwsRequestSigningConstants.Algorithm}\n{amzDate}\n{scope}\n{canonicalRequestHash}";
        }
    }

}
