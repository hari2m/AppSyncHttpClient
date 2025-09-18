namespace AppSyncHttpClient.Models
{
    public class PublishPayload<T>
    {
        public string channel { get; set; } = default!;
        public T[] events { get; set; } = default!;
    }
}
