using System.Threading.Tasks;

namespace Efflux
{
    public interface ITopic
    {
        string TopicName { get; }

        Task<ITopicConsumer> CreateConsumerAsync(string ConsumerName, long startOffset = 0);
        Task<MessageReadResult> ReadMessageAsync(long offset);
        Task<long> WriteMessageAsync(EffluxMessage message);

        // Retrieve a list of messages from index only
        // 
        Task<EffluxMessage> FindByIdAsync(string id, bool consumeMessage = true);
    }
}