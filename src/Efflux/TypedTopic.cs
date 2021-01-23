using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efflux
{
    public class TypedTopic<T> : ITopic where T : class
    {
        readonly ITopic topic;

        public string TopicName => topic.TopicName;

        public TypedTopic(ITopic topic)
        {
            this.topic = topic;
        }

        public async Task<TypedTopicConsumer<T>> CreateConsumerAsync(string ConsumerName, long startOffset = 0) =>
            new TypedTopicConsumer<T>(await topic.CreateConsumerAsync(ConsumerName, startOffset));

        public async Task<T> FindByIdAsync(string id)
        {
            var message = await topic.FindByIdAsync(id);

            if (message == null)
                return null;

            var value = JsonSerializer.Deserialize<T>(message.PayloadAsBytes());

            return value;
        }

        public Task<long> AppendAsync(T value)
        {
            var data = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(data);
            var message = new EffluxMessage(bytes, "JSON");
            return topic.WriteMessageAsync(message);
        }

        Task<ITopicConsumer> ITopic.CreateConsumerAsync(string ConsumerName, long startOffset)
        {
            return topic.CreateConsumerAsync(ConsumerName, startOffset);
        }

        Task<MessageReadResult> ITopic.ReadMessageAsync(long offset)
        {
            return topic.ReadMessageAsync(offset);
        }

        Task<long> ITopic.WriteMessageAsync(EffluxMessage message)
        {
            return topic.WriteMessageAsync(message);
        }

        Task<EffluxMessage> ITopic.FindByIdAsync(string id, bool consumeMessage)
        {
            return topic.FindByIdAsync(id, consumeMessage);
        }
    }
}