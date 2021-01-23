using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Efflux
{
    public class TypedTopicConsumer<T> : IAsyncEnumerable<T>, ITopicConsumer where T : class
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
            var result = new MessageReadResult<T>
            {
                MessageData = readResult.Message.PayloadAs<T>(),
                MetaData = readResult.Message.MetaData,
                EndOfStream = readResult.EndOfStream,
                NextMessageOffset = readResult.NextMessageOffset,
                Ticket = readResult.Ticket
            };
            return result;
        }

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken) => new TopicEnumerator<T>(consumer);

        Task<MessageReadResult> ITopicConsumer.ReadMessageAsync(bool autoCommit)
        {
            return consumer.ReadMessageAsync(autoCommit);
        }
    }
}