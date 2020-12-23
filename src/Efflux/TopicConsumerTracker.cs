using System;

namespace Efflux
{
    public class TopicConsumerTracker
    {
        public TopicConsumerTracker()
        {
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public long StartOffset { get; set; }
        public long CurrentOffset { get; set; }
    }
}
