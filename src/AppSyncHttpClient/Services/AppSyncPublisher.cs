using AppSyncHttpClient.Interface;

namespace AppSyncHttpClient.Services
{
    public class AppSyncPublisher : IAppSyncPublisher
    {
        private readonly AppSyncPublishAction _publishAction;

        public AppSyncPublisher(AppSyncPublishAction publishAction)
        {
            _publishAction = publishAction ?? throw new ArgumentNullException(nameof(publishAction));
        }

        public Task PublishAsync<T>(string topic, T message)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic must not be null or empty", nameof(topic));

            return _publishAction.ExecuteAsync(topic, message);
        }
    }
}
