using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efflux
{
    public interface ITopicConsumer
    {
        string Name { get; }
        long StartOffset { get; }
        long CurrentOffset { get; }

        bool Commit(string ticketId);
        CommitTicket Extend(string ticketId);
        Task<MessageReadResult> ReadMessageAsync(bool autoCommit = false);
        bool Rollback(string ticketId);
    }
}