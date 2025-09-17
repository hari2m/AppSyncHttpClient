namespace AppSyncHttpClient.Constants
{
    internal class AwsRequestSigningConstants
    {
        public const string Algorithm = "AWS4-HMAC-SHA256";
        public const string Service = "appsync";
        public const string Aws4Request = "aws4_request";

        public static class Headers
        {
            public const string ContentEncoding = "content-encoding";
            public const string ContentType = "content-type";
            public const string Host = "host";
            public const string AmzContentSha256 = "x-amz-content-sha256";
            public const string AmzDate = "x-amz-date";
            public const string AmzSecurityToken = "x-amz-security-token";
            public const string Authorization = "Authorization";
        }
    }
}
