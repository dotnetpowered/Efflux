using System;

namespace Efflux
{
    public class CommitTicket
    {
        public CommitTicket()
        {
        }

        public string Id { get; set; }
        public string ConsumerName { get; set; }
        public DateTime Dequeued { get; set; }
        public DateTime Expiration { get; set; }
        public long Offset { get; set; }
    }
}
