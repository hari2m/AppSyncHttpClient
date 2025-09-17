namespace AppSyncHttpClient.Interface;

public interface IAppSyncPublisher
{
    Task PublishAsync<T>(string topic, T message);
}
