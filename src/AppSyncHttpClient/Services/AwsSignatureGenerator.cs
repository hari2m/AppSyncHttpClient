using Amazon.Runtime;
using AppSyncHttpClient.Interface;
using AppSyncHttpClient.Utilities;
using System.Security.Cryptography;
using System.Text;

namespace AppSyncHttpClient.Services
{
    public class AwsSignatureGenerator : IAwsSignatureGenerator
    {
        public string GenerateSignature(ImmutableCredentials credentials, string region, string date, string service, string stringToSign)
        {
            byte[] kDate = Hmac($"AWS4{credentials.SecretKey}", date);
            byte[] kRegion = Hmac(kDate, region);
            byte[] kService = Hmac(kRegion, service);
            byte[] kSigning = Hmac(kService, "aws4_request");

            return HexConverter.ToHex(Hmac(kSigning, stringToSign));
        }

        private byte[] Hmac(string key, string data) => Hmac(Encoding.UTF8.GetBytes(key), data);

        private byte[] Hmac(byte[] key, string data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
