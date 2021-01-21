using System;
using System.Threading.Tasks;

namespace Efflux.Samples.TypedDocument
{
    public class TypedDocumentSample
    {
        private readonly ITopicFactory topicFactory;

        public TypedDocumentSample(ITopicFactory topicFactory)
        {
            this.topicFactory = topicFactory;
        }

        public async Task TypedDocumentDemo()
        {
            Console.WriteLine("Efflux TypedDocument Sample");

            var topic3 = topicFactory.OpenTopic<Person>("topic3");

            await topic3.AppendAsync(new Person() { FullName = "Mad Dog" });
            var consumer3 = await topic3.CreateConsumerAsync("consumer3");

            Console.WriteLine($"Starting consumer at {consumer3.CurrentOffset}");

            await foreach (var p in consumer3)
            {
                Console.WriteLine(p.FullName);
            }

            MessageReadResult<Person> readResult = null;
            TypedTopicConsumer<Person> consumer = await topic3.CreateConsumerAsync("consumer3-1");
            Console.WriteLine($"Reading Topic 3 (typed document) - Starting consumer at {consumer.CurrentOffset}");

            while (!readResult.EndOfStream)
            {
                Console.WriteLine("  reading...");
                readResult = await consumer.ReadAsync(true);
                if (readResult != null)
                    Console.WriteLine(readResult.MessageData.FullName);
            }

        }
    }
}
