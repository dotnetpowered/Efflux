using System;
using System.Threading.Tasks;

namespace Efflux.Samples.Messaging
{
    public static class MessagingSample
    {
        public static async Task Demo(ITopic topic)
        {
            // Message Stream access method (write to end, random read)
            // ------------------------------------------------------------
            var start = DateTime.Now;
            long readOffset = 0;
            for (var i = 0; i < 100; i++)
            {
                var message = new EffluxMessage("hello world Booen39gb9j23binakd " + i);
                var offset = await topic.WriteMessageAsync(message);

                Console.WriteLine($"Wrote {i}: {message.MetaData.Id} at {offset}");

                if (i == 50)
                {
                    readOffset = offset;
                }
            }
            Console.WriteLine($"Write Duration: {(DateTime.Now - start).TotalMilliseconds / 1000.0}s");

            var readResult = await topic.ReadMessageAsync(readOffset);
            Console.WriteLine($"Read from {readOffset} found id={readResult.Message.MetaData.Id} data='{readResult.Message.PayloadAsString()}'");

            // Consumer access method (resumable forward-only reader)
            // ------------------------------------------------------------
            Console.WriteLine("Starting consumer:");
            MessageReadResult result = new MessageReadResult();
            var consumer = await topic.CreateConsumerAsync("consumer1");
            Console.WriteLine($"Starting consumer at {consumer.CurrentOffset}");

            while (!result.EndOfStream)
            {
                result = await consumer.ReadMessageAsync(true);
                if (result.Message != null)
                    Console.WriteLine(result.Message.PayloadAsString());
            }
        }
    }
}
