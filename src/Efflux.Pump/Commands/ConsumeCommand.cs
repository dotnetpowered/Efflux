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

        public async Task ConsumeMessage(string topic, string consumerName, int offset, int count, bool verbose)
        {
            if (verbose)
            {
                if (offset > 0)
                    Console.WriteLine($"Reading message from topic '{topic}' using consumer '{consumerName}' at offset {offset}");
                else
                    Console.WriteLine($"Reading message from topic '{topic}' using consumer '{consumerName}'");
            }
            var topicStream = topicFactory.OpenTopic(topic);
            var consumer = await topicStream.CreateConsumerAsync(consumerName, offset);

            for (int i=0;i<count;i++)
            {
                var result = await consumer.ReadMessageAsync(true);
                if (result.EndOfStream)
                {
                    Console.WriteLine("<End of Stream>");
                    return;
                }
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine($"  Id: {result.Message.MetaData.Id}");
                Console.WriteLine($"  Created: {result.Message.MetaData.Created}");
                Console.WriteLine($"  Content-Type: {result.Message.MetaData.ContentType}");
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine(result.Message.PayloadAsString());
            }

        }
    }
}
