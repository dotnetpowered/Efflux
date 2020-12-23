using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Efflux.Data
{
    public class TopicDataView : IAsyncEnumerable<IDataRow> 
    {
        readonly ITopicConsumer consumer;
        readonly DataSchema schema;

        public TopicDataView(ITopicConsumer consumer, DataSchema schema)
        {
            this.schema = schema;
            this.consumer = consumer;
        }

        public TopicDataReader AsDataReader()
        {
            return new TopicDataReader(consumer, schema);
        }

        // IAsyncEnumerable<IDataRow>

        public IAsyncEnumerator<IDataRow> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TopicDataEnumerator(consumer, schema);
        }
    }
}
