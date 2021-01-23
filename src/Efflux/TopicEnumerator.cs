using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efflux
{
    public class TopicEnumerator<T> : IAsyncEnumerator<T> where T:class
    {
        readonly ITopicConsumer consumer;

        public TopicEnumerator(ITopicConsumer consumer)
        {
            this.consumer = consumer; 
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            var readResult = await consumer.ReadMessageAsync();
            if (!readResult.EndOfStream)
                Current = readResult.Message.PayloadAs<T>();
            else
                Current = null;

            Position = readResult.NextMessageOffset;
            return !readResult.EndOfStream;
        }

        public void Reset()
        {
            // TODO: no op?
        }

        public ValueTask DisposeAsync()
        {
            // TODO: no op?
            return ValueTask.CompletedTask;
        }

        public T Current { get; private set; }
        public long Position { get; set; }
    }
}
