using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Efflux
{
    public class TopicCache
    {
        private readonly ConcurrentDictionary<string, ITopic> cache = new();
        private readonly ILogger<TopicCache> _logger;
        private readonly ITopicFactory topicFactory;

        public TopicCache(ILogger<TopicCache> logger, ITopicFactory topicFactory)
        {
            _logger = logger;
            this.topicFactory = topicFactory;
        }

        public ITopic Get(string topicName)
        {
            _logger.LogInformation($"Open stream {topicName}");
            return cache.GetOrAdd(topicName, (key) =>
                        {
                            return topicFactory.OpenTopic(key);
                        });
        }

        public TypedTopic<T> Get<T>(string topicName) where T: class
        {
            _logger.LogInformation($"Open stream {topicName}");
            return new TypedTopic<T>(cache.GetOrAdd(topicName, (key) =>
            {
                return topicFactory.OpenTopic(key);
            }));
        }
    }
}
