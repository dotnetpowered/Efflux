using System;
using System.Collections.Generic;

namespace Efflux
{
    public class EffluxMetaData //: IEffluxMetaData
    {
        public EffluxMetaData()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        // UTC DateTime when Message was created
        public DateTime Created { get; set; }

        // UTC DateTime when Data was created
        public DateTime Timestamp { get; set; }

        // SHA256 Hash of data  
        public byte[] PayloadHash { get; set; }

        // Fingerprint of previous record
        public byte[] LinkedFingerprint { get; set; }

        // Application-specific data type 
        public string DataType { get; set; }

        // application/json, application/octet-stream, text/plain;UTF8 
        public string ContentType { get; set; }

        // gzip
        public string ContentEncoding { get; set; }
        public IDictionary<string, string> Properties { get => properties; set => properties = value; }
        public string MessageGroup { get; set; }

        private IDictionary<string, string> properties = new Dictionary<string, string>();

        // Compression
        // https://github.com/ImpromptuNinjas/ZStd
        // https://facebook.github.io/zstd/

        // Encryption
    }

}