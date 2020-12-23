using System;
using System.Collections.Generic;

namespace Efflux
{
    public class IndexedMessageMetaData 
    {
        public IndexedMessageMetaData()
        {
        }

        public IndexedMessageMetaData(EffluxMetaData messageMetaData, long Offset, long Size)
        {
            this.Id = messageMetaData.Id;
            this.Offset = Offset;
            this.Timestamp = messageMetaData.Timestamp;
            this.DataType = messageMetaData.DataType;
            this.ContentType = messageMetaData.ContentType;
            this.Properties = messageMetaData.Properties;
            this.Size = Size;
            this.MessageGroup = messageMetaData.MessageGroup;
        }

        public string Id { get; set; }

        public long Offset { get; set; }

        // UTC Date Time when record was created
        public DateTime Timestamp { get; set; }

        // Application-specific data type 
        public string DataType { get; set; }

        // applicatinon/json, application/octet-stream, text/plain;UTF8, 
        public string ContentType { get; set; }

        public long Size { get; set; }

        public string MessageGroup { get; set; }

        public IDictionary<string, string> Properties = new Dictionary<string, string>();
    }

}