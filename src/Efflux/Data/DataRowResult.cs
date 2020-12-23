namespace Efflux.Data
{  
    public class DataRowResult
    {
        public IDataRow Row { get; set; }
        public CommitTicket Ticket { get; set; }
        public long NextOffset { get; set; }
        public bool EndOfStream { get; set; }
    }
}