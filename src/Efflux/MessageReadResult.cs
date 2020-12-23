namespace Efflux
{  
    public class MessageReadResult
    {
        public EffluxMessage Message { get; set; }
        public CommitTicket Ticket { get; set; }
        public long NextMessageOffset { get; set; }
        public bool EndOfStream { get; set; }
    }

    public class MessageReadResult<T> where T: class
    {
        public T MessageData { get; set; }
        public EffluxMetaData MetaData { get; set; }
        public CommitTicket Ticket { get; set; }
        public long NextMessageOffset { get; set; }
        public bool EndOfStream { get; set; }
    }
}