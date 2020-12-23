using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Efflux
{
    public class TypedTopicConsumer<T> : IAsyncEnumerable<T> where T : class
    {
        readonly ITopicConsumer consumer;

        public TypedTopicConsumer(ITopicConsumer consumer)
        {
            this.consumer = consumer;
        }

        public string Name { get => consumer.Name; set => Name = consumer.Name; }
        public long StartOffset { get => consumer.StartOffset; set => StartOffset = consumer.StartOffset; }
        public long CurrentOffset => consumer.CurrentOffset;

        public bool Commit(string ticketId) => consumer.Commit(ticketId);
        public CommitTicket Extend(string ticketId) => consumer.Extend(ticketId);
        public bool Rollback(string ticketId) => consumer.Rollback(ticketId);

        public async Task<MessageReadResult<T>> ReadAsync(bool autoCommit = false)
        {
            var readResult = await consumer.ReadMessageAsync(autoCommit);
            var result = new MessageReadResult<T>();
            result.MessageData = readResult.Message.PayloadAs<T>();
            result.MetaData = readResult.Message.MetaData;
            result.EndOfStream = readResult.EndOfStream;
            result.NextMessageOffset = readResult.NextMessageOffset;
            result.Ticket = readResult.Ticket;
            return result;
        }

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TopicEnumerator<T>(consumer);
    }
}