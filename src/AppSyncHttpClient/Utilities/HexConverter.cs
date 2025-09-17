namespace AppSyncHttpClient.Utilities
{
    public static class HexConverter
    {
        public static string ToHex(byte[] bytes) =>
            BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}