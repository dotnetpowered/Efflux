namespace Efflux
{
    public interface ITopicFactory
    {
        ITopic OpenTopic(string TopicName);
    }

    public static class TypedTopicFactory
    {
        public static TypedTopic<T> OpenTopic<T>(this ITopicFactory topicFactory, string TopicName) where T : class
        {
            return new TypedTopic<T>(topicFactory.OpenTopic(TopicName));
        }
    }
}
