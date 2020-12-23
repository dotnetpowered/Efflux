using System;
using Microsoft.Extensions.Logging;

namespace Efflux
{
    public interface ITopicFactory
    {
        ITopic OpenTopic(ILogger logger, string TopicName);
    }
}
