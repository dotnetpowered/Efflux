using System;
using System.Threading.Tasks;

namespace Efflux.Pump.Commands
{
    public class PublishCommand
    {
        private readonly ITopicFactory topicFactory;

        public PublishCommand(ITopicFactory topicFactory)
        {
            this.topicFactory = topicFactory;
        }

        public async Task PublishMessage(string topic, string messagePayload, bool verbose)
        {
            if (verbose)
                Console.WriteLine($"Publishing new message to topic '{topic}'");
            var topicStream = topicFactory.OpenTopic(topic);
            var message = new EffluxMessage(messagePayload);
            var offset = await topicStream.WriteMessageAsync(message);
            if (verbose)
                Console.WriteLine($"Message written to offset {offset}");
        }
    }
}
