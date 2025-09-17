namespace AppSyncHttpClient.Models
{
    public class PublishPayload<T>
    {
        public string Channel { get; set; } = default!;
        public T[] Events { get; set; } = default!;
    }
}
