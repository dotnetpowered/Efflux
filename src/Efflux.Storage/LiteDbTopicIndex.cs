using System;
using System.Linq;
using LiteDB;

namespace Efflux.Stream
{
    public class LiteDbTopicIndex : ITopicIndex
    {
        string TopicName;
        LiteDatabase db;

        ILiteCollection<IndexedMessageMetaData> messages;
        ILiteCollection<TopicConsumerTracker> consumers;
        ILiteCollection<CommitTicket> tickets;

        public LiteDbTopicIndex()
        {
        }

        public ITopicIndex Open(string topicName)
        {
            this.TopicName = topicName;
            db = new LiteDatabase(TopicName + "/index.db");
            db.Checkpoint();
            messages = db.GetCollection<IndexedMessageMetaData>("messages");
            consumers = db.GetCollection<TopicConsumerTracker>("consumers");
            tickets = db.GetCollection<CommitTicket>("tickets");
            return this;
        }

        public IndexedMessageMetaData FindMessage(string id)
        {
            return messages.Find(x => x.Id == id).FirstOrDefault();
        }

        public void InsertMessageMetaData(IndexedMessageMetaData doc)
        {
            messages.Insert(doc);
        }

        public TopicConsumerTracker GetConsumer(string ConsumerName)
        {
            consumers.EnsureIndex(x => x.Name, true);

            TopicConsumerTracker consumer = consumers.FindOne(c => c.Name == ConsumerName);

            return consumer;
        }

        public void AddConsumer(TopicConsumerTracker consumer)
        {
            consumers.Insert(consumer);
        }

        public void AddTicket(CommitTicket ticket)
        {
            tickets.Insert(ticket);
        }

        public void UpdateTicket(CommitTicket ticket)
        {
            tickets.Update(ticket);
        }

        public bool DeleteTicket(string ticketId)
        {
            return tickets.Delete(ticketId);
        }

        public CommitTicket GetTicket(string ticketId)
        {
            return tickets.Find(t => t.Id == ticketId).FirstOrDefault();
        }

        public CommitTicket GetNextTicket(string consumerName)
        {
            var ticket = tickets.Find(t => t.Expiration < DateTime.UtcNow &&
                            t.ConsumerName == consumerName)
                    .OrderBy(t => t.Expiration)
                    .FirstOrDefault();
            return ticket;
        }
    }
}
