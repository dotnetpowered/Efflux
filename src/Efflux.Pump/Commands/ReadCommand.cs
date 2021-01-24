using System;
using System.Threading.Tasks;

namespace Efflux.Pump.Commands
{
    public class ReadCommand
    {
        private readonly ITopicFactory topicFactory;

        public ReadCommand(ITopicFactory topicFactory)
        {
            this.topicFactory = topicFactory;
        }

        public async Task ReadMessage(string topic, long offset, bool verbose)
        {
            if (verbose)
                Console.WriteLine($"Reading message from topic '{topic}' at offset '{offset}'");
            var topicStream = topicFactory.OpenTopic(topic);
            var result = await topicStream.ReadMessageAsync(offset);
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
