using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FASTER.core;
using LiteDB;

namespace Efflux.Stream
{
    public class TopicStreamConsumer : ITopicConsumer
    {
        internal TopicStreamConsumer(TopicStream topicStream,
            TopicConsumerTracker tracker, ITopicIndex topicIndex,
            FasterLogScanIterator fasterLogScanIterator)
        {
            this.Id = tracker.Id;
            this.CurrentOffset = tracker.CurrentOffset;
            this.Name = tracker.Name;
            this.StartOffset = tracker.StartOffset;
            this.Topic = topicStream;
            this.TopicIndex = topicIndex;
            this.iter = fasterLogScanIterator;
        }

        private TopicStream Topic { get; set; }
        private FasterLogScanIterator iter { get; set; }
        private ITopicIndex TopicIndex { get; set; }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public long StartOffset { get; private set; }
        public long CurrentOffset { get; internal set; }


        public bool Commit(string ticketId)
        {
            return TopicIndex.DeleteTicket(ticketId);
        }

        public bool Rollback(string ticketId)
        {
            var ticket = TopicIndex.GetTicket(ticketId);
            if (ticket == null)
                return false;
            else
            {
                ticket.Expiration = DateTime.MinValue;
                TopicIndex.UpdateTicket(ticket);
                return true;
            }
        }

        public CommitTicket Extend(string ticketId)
        {
            var ticket = TopicIndex.GetTicket(ticketId);
            if (ticket == null)
                return null;
            else
            {
                ticket.Expiration = DateTime.UtcNow.AddSeconds(90);
                TopicIndex.UpdateTicket(ticket);
                return ticket;
            }
        }

        public async Task<MessageReadResult> ReadMessageAsync(bool autoCommit = false)
        {
            var ticket = TopicIndex.GetNextTicket(Name);
            MessageReadResult readResult;
            string deleteTicketId = null;
            if (ticket != null)
            {
                deleteTicketId = ticket?.Id;

                long currentOffset = ticket?.Offset ?? CurrentOffset;
                if (!autoCommit)
                {
                    if (ticket != null)
                    {
                        ticket = ExtendTicket(ticket);
                    }
                    else
                    {
                        ticket = GetCommitTicket();
                    }
                }

                readResult = await Topic.ReadMessageAsync(currentOffset);
            }
            else
            {
                var hasData = iter.GetNext(out byte[] result, out int len, out long currentAddress, out long nextAddress);
                if (hasData)
                {
                    var message = EffluxMessage.FromStream(new MemoryStream(result));
                    if (message.PayloadAsString() == "Root")
                    {
                        iter.CompleteUntil(nextAddress);
                        hasData = iter.GetNext(out result, out len, out currentAddress, out nextAddress);
                        if (hasData)
                        {
                            message = EffluxMessage.FromStream(new MemoryStream(result));
                        }
                    }
                    if (hasData)
                    {
                        readResult = new MessageReadResult()
                        {
                            EndOfStream = false,
                            NextMessageOffset = nextAddress,
                            Message = message
                        };
                        if (!autoCommit)
                        {
                            ticket = GetCommitTicket();
                        }
                    }
                    else
                    {
                        readResult = new MessageReadResult()
                        {
                            EndOfStream = true
                        };
                    }
                    iter.CompleteUntil(nextAddress);
                    await this.Topic.log.CommitAsync();
                }
                else
                {
                    readResult = new MessageReadResult()
                    {
                        EndOfStream = true
                    };
                }
            }

            if (!readResult.EndOfStream)
            {
                CurrentOffset = readResult.NextMessageOffset;
                if (deleteTicketId != null)
                    TopicIndex.DeleteTicket(deleteTicketId);
                if (!autoCommit)
                {
                    TopicIndex.AddTicket(ticket);
                    readResult.Ticket = ticket;
                }
                //Do not need to update DB as the log keeps track of this via
                // the iter.CompleteUtil and CommitAsync calls above.
                //Topic.consumers.Update(this);
            }


            return readResult;
        }


        CommitTicket GetCommitTicket()
        {
            var ticket = new CommitTicket()
            {
                Dequeued = DateTime.UtcNow,
                Offset = CurrentOffset,
                Id = ObjectId.NewObjectId().ToString(),
                ConsumerName = Name
            };
            ticket.Expiration = ticket.Dequeued.AddSeconds(90);
            return ticket;
        }

        CommitTicket ExtendTicket(CommitTicket t)
        {
            var ticket = new CommitTicket()
            {
                Dequeued = DateTime.UtcNow,
                Offset = t.Offset,
                Id = ObjectId.NewObjectId().ToString(),
                ConsumerName = t.ConsumerName
            };
            ticket.Expiration = ticket.Dequeued.AddSeconds(90);
            return ticket;
        }
    }


}
