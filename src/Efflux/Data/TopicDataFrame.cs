using System;
using System.Threading.Tasks;

namespace Efflux.Data
{
    public class TopicDataFrame
    {
        ITopic topic;
        DataSchema schema;

        public TopicDataFrame(ITopic t, DataSchema schema)
        {
            this.topic = t;
            this.schema = schema;
        }

        public async Task<TopicDataView> GetDataViewAsync(string consumerName, long startOffset=0) =>
            new TopicDataView(
                await topic.CreateConsumerAsync(consumerName, startOffset),
                schema);

        public async Task<long> AppendAsync(IDataRow row)
        {
            EffluxMessage message = row.ToMessage();
            return await topic.WriteMessageAsync(message);
        }
    }
}
