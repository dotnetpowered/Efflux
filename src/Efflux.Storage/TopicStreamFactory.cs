using System;
using Microsoft.Extensions.Logging;

namespace Efflux.Stream
{
    public class TopicStreamFactory : ITopicFactory
    {
        private readonly ILogger logger;

        public TopicStreamFactory(ILogger logger)
        {
            this.logger = logger;
        }

        public ITopic OpenTopic(string TopicName)
        {
            var result = TopicStream.OpenAsync(logger, TopicName, //new LiteDbTopicIndex());
                new TopicIndexWithLog(new LiteDbTopicIndex(), logger));
            result.Wait();
            return result.Result;
        }
    }
}
