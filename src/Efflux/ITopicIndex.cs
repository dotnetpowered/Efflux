namespace Efflux
{
    public interface ITopicIndex
    {
        ITopicIndex Open(string topicName);
        void AddConsumer(TopicConsumerTracker consumer);
        void AddTicket(CommitTicket ticket);
        bool DeleteTicket(string ticketId);
        IndexedMessageMetaData FindMessage(string id);
        TopicConsumerTracker GetConsumer(string ConsumerName);
        CommitTicket GetNextTicket(string consumerName);
        CommitTicket GetTicket(string ticketId);
        void InsertMessageMetaData(IndexedMessageMetaData doc);
        void UpdateTicket(CommitTicket ticket);
    }
}