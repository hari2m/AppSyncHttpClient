using Amazon.Runtime;

namespace AppSyncHttpClient.Interface
{
    public interface IAwsSignatureGenerator
    {
        string GenerateSignature(ImmutableCredentials credentials, string region, string date, string service, string stringToSign);
    }
}
