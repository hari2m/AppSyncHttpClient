namespace AppSyncHttpClient.Utilities
{
    public static class HttpHeaderHelper
    {
        public static string GetHeader(HttpRequestMessage request, string name)
        {
            return request.Headers.TryGetValues(name, out var h)
                ? string.Join(",", h)
                : request.Content?.Headers.TryGetValues(name, out var ch) == true
                    ? string.Join(",", ch)
                    : string.Empty;
        }
    }
}
