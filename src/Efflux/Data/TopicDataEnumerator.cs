using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Efflux.Data
{
    public class TopicDataEnumerator : IAsyncEnumerator<IDataRow> 
    {
        DataSchema schema;
        ITopicConsumer consumer;

        internal TopicDataEnumerator(ITopicConsumer consumer, DataSchema schema)
        {
            this.schema = schema;
            this.consumer = consumer;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            var readResult = await consumer.ReadDataRowAsync();
            Current = readResult.Row;
            Position = readResult.NextOffset;
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

        public IDataRow Current { get; private set; }
        public long Position { get; set; }
    }
}
