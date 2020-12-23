using System;
using Microsoft.Extensions.Logging;

namespace Efflux.Stream
{
    public class TopicIndexWithLog : ITopicIndex
    {
        ITopicIndex topicIndex;
        BasicStream topicIndexLog;
        ILogger logger;

        public TopicIndexWithLog(ITopicIndex topicIndex, ILogger logger)
        {
            this.topicIndex = topicIndex;
            this.logger = logger;
        }

        public ITopicIndex Open(string topicName)
        {
            topicIndex.Open(topicName);
            var openResult = BasicStream.OpenAsync(logger, topicName + "/index-stream");
            openResult.Wait();
            topicIndexLog = openResult.Result;
            return this;
        }

        public void AddConsumer(TopicConsumerTracker consumer)
        {
            var logEntry = new LogEntry<TopicConsumerTracker> { Action = "AddConsumer", Data = consumer };
            var message = EffluxMessage.From(logEntry);
            var task = topicIndexLog.WriteMessageAsync(message);
            task.Wait();
            topicIndex.AddConsumer(consumer);
        }

        public void AddTicket(CommitTicket ticket)
        {
            var logEntry = new LogEntry<CommitTicket> { Action = "AddTicket", Data = ticket };
            var message = EffluxMessage.From(logEntry);
            var task = topicIndexLog.WriteMessageAsync(message);
            task.Wait();
            topicIndex.AddTicket(ticket);
        }

        public bool DeleteTicket(string ticketId)
        {
            var logEntry = new LogEntry<string> { Action = "DeleteTicket", Data = ticketId };
            var message = EffluxMessage.From(logEntry);
            var task = topicIndexLog.WriteMessageAsync(message);
            task.Wait();
            return topicIndex.DeleteTicket(ticketId);
        }

        public void InsertMessageMetaData(IndexedMessageMetaData doc)
        {
            var logEntry = new LogEntry<IndexedMessageMetaData> { Action = "InsertMessage", Data = doc };
            var message = EffluxMessage.From(logEntry);
            var task = topicIndexLog.WriteMessageAsync(message);
            task.Wait();
            topicIndex.InsertMessageMetaData(doc);
        }

        public void UpdateTicket(CommitTicket ticket)
        {
            var logEntry = new LogEntry<CommitTicket> { Action = "UpdateTicket", Data = ticket };
            var message = EffluxMessage.From(logEntry);
            var task = topicIndexLog.WriteMessageAsync(message);
            task.Wait();
            topicIndex.UpdateTicket(ticket);
        }

        // Read methods are simpling pass-thru

        public IndexedMessageMetaData FindMessage(string id)
        {
            return topicIndex.FindMessage(id);
        }

        public TopicConsumerTracker GetConsumer(string ConsumerName)
        {
            return topicIndex.GetConsumer(ConsumerName);
        }

        public CommitTicket GetNextTicket(string consumerName)
        {
            return topicIndex.GetNextTicket(consumerName);
        }

        public CommitTicket GetTicket(string ticketId)
        {
            return topicIndex.GetTicket(ticketId);
        }
    }
}
