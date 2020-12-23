using System;

namespace Efflux.Stream
{
    public class LogEntry<T>
    {
        public LogEntry()
        {
        }

        public string Action { get; set; }
        public T Data { get; set; }

    }
}
