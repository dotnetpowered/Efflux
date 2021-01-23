using System;
using System.Threading.Tasks;

namespace Efflux.Pump.Commands
{
    public class ConsumeCommand
    {
        private readonly ITopicFactory topicFactory;

        public ConsumeCommand(ITopicFactory topicFactory)
        {
            this.topicFactory = topicFactory;
        }

        public async Task ConsumeMessage(string topic, string consumerName, bool verbose)
        {
            if (verbose)
                Console.WriteLine($"Reading message from topic '{topic}' using consumer '{consumerName}'");
            var topicStream = topicFactory.OpenTopic(topic);
            var consumer = await topicStream.CreateConsumerAsync(consumerName);
            var result = await consumer.ReadMessageAsync(true);
            if (result.EndOfStream)
            {
                Console.WriteLine("<End of Stream>");
                return;
            }
            if (verbose)
            {
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine($"  Id: {result.Message.MetaData.Id}");
                Console.WriteLine($"  Created: {result.Message.MetaData.Created}");
                Console.WriteLine($"  Content-Type: {result.Message.MetaData.ContentType}");
                Console.WriteLine("----------------------------------------------------");
            }
            Console.WriteLine(result.Message.PayloadAsString());
        }
    }
}
