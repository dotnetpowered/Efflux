using System;
using Microsoft.Extensions.Logging;

namespace Efflux.Stream
{
    public class TopicStreamFactory : ITopicFactory
    {
        public TopicStreamFactory()
        {
        }

        public ITopic OpenTopic(ILogger logger, string TopicName)
        {
            var result = TopicStream.OpenAsync(logger, TopicName, //new LiteDbTopicIndex());
                new TopicIndexWithLog(new LiteDbTopicIndex(), logger));
            result.Wait();
            return result.Result;
        }
    }
}
