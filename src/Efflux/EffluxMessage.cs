using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Efflux
{
    public class EffluxMessage //: IEffluxMessage
    {
        public static readonly byte[] HeaderID = new byte[] { 5, 1, 20, 41, 131, 10 };
        public static readonly byte[] FooterID = new byte[] { 6, 17, 82, 91 };

        // SHA-256 hash of metadata
        public byte[] Fingerprint { get; internal set; }
        public EffluxMetaData MetaData { get; internal set; }
        private byte[] PayloadStorage;

        // Uncompressed size of DataBlock (might be different that what is in DataBlockStorage)
        private long? _PayloadSize;

        static readonly SHA256 sha256Hash = SHA256.Create();

        public EffluxMessage()
        {
            this.MetaData = new EffluxMetaData();
        }

        public EffluxMessage(ReadOnlySpan<byte> bytes, string DataFormat = "application/octet-stream", DateTime? Timestamp = null) : this()
        {
            int Threshold = 512;
            if (bytes.Length > Threshold)
            {
                MetaData.ContentEncoding = "gzip";
                var inputBytes = bytes.ToArray();
                var stream = new GZipStream(new MemoryStream(inputBytes), CompressionLevel.Fastest);
                var comStream = new MemoryStream();
                stream.CopyTo(comStream);
                PayloadStorage = comStream.ToArray();
                _PayloadSize = inputBytes.Length;
            }
            else
            {
                PayloadStorage = bytes.ToArray();
                _PayloadSize = PayloadStorage.Length;
            }
            MetaData.PayloadHash = sha256Hash.ComputeHash(PayloadStorage);
            MetaData.Created = DateTime.UtcNow;
            MetaData.Timestamp = Timestamp ?? MetaData.Created;
            MetaData.ContentType = DataFormat;
        }

        public EffluxMessage(string s) : this(Encoding.UTF8.GetBytes(s), "text/plain;UTF8")
        {
        }

        public EffluxMessage(string s, string format) : this(Encoding.UTF8.GetBytes(s), format)
        {
        }

        public long DataBlockSize
        {
            get
            {
                if (_PayloadSize == null)
                {
                    PayloadAsBytes(); // Forces _PayloadSize to be calculated
                }
                return (long) _PayloadSize;
            }
        }

        public IEnumerable<byte> ToBytes()
        {
            byte[] metaDataBuffer = JsonSerializer.SerializeToUtf8Bytes(MetaData, new JsonSerializerOptions() { IgnoreNullValues = true });

            this.Fingerprint = sha256Hash.ComputeHash(metaDataBuffer);

            int MetaDataLength = metaDataBuffer.Length;
            int Length = PayloadStorage.Length + metaDataBuffer.Length + Fingerprint.Length + sizeof(int) * 3;

            byte[] lengthBuffer = BitConverter.GetBytes(Length);
            byte[] metaDatalengthBuffer = BitConverter.GetBytes(MetaDataLength);

            return HeaderID                   // 6 bytes
                .Concat(lengthBuffer)         // 4 bytes
                .Concat(metaDatalengthBuffer) // 4 bytes
                .Concat(Fingerprint)          // 32 bytes
                .Concat(metaDataBuffer)       // Variable length
                .Concat(PayloadStorage)     // Variable length
                .Concat(lengthBuffer)         // 4 bytes
                .Concat(FooterID);            // 4 bytes
        }

        public static EffluxMessage FromStreamPriorMessage(Stream stream, SeekOrigin seekOrigin)
        {
            var reader = new BinaryReader(stream);
            stream.Seek(-4, seekOrigin);
            var footerID = reader.ReadBytes(FooterID.Length);  // 4 bytes
            if (!footerID.SequenceEqual(FooterID))
                throw new InvalidDataException("Invalid message footer");
            stream.Seek(-8, SeekOrigin.Current);
            var length = reader.ReadInt32();                   // 4 bytes
            stream.Seek(-length - 6, SeekOrigin.Current);
            return FromStream(stream);
        }

        public static EffluxMessage FromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var headerID = reader.ReadBytes(HeaderID.Length);  // 6 bytes
            if (!headerID.SequenceEqual(HeaderID))
                throw new InvalidDataException("Invalid message header");
            var length = reader.ReadInt32();                   // 4 bytes
            var metadataLength = reader.ReadInt32();           // 4 bytes
            var fingerprint = reader.ReadBytes(32);            // 32 bytes (SHA256 hash)
            var metadataBuffer = reader.ReadBytes(metadataLength);  // variable length
            var dataLength = length - fingerprint.Length
                                    - metadataLength
                                    - sizeof(int) * 3;
            var data = reader.ReadBytes(dataLength);           // variable length
            var length2 = reader.ReadInt32();                  // 4 bytes
            if (length != length2)
                throw new InvalidDataException("Length mismatch");
            var footerID = reader.ReadBytes(FooterID.Length);  // 4 bytes
            if (!footerID.SequenceEqual(FooterID))
                throw new InvalidDataException("Invalid message footer");

            // Verify fingerprint stored in record matches one calculated from meta data    
            var computedFingerprint = sha256Hash.ComputeHash(metadataBuffer);
            if (!fingerprint.SequenceEqual(computedFingerprint))
                throw new InvalidDataException("Invalid fingerprint");

            // Deserialize metadata
            var metadata = JsonSerializer.Deserialize<EffluxMetaData>(metadataBuffer);

            // Verify hash stored in metadata matches one calculated from data    
            var computedDataHash = sha256Hash.ComputeHash(data);
            if (!computedDataHash.SequenceEqual(metadata.PayloadHash))
                throw new InvalidDataException("Invalid data hash");

            var record = new EffluxMessage()
            {
                MetaData = metadata,
                PayloadStorage = data,
                Fingerprint = fingerprint
            };

            return record;
        }

        public static EffluxMessage From<T>(T Object) where T: class
        {
            return new EffluxMessage(JsonSerializer.Serialize(Object), "application/json");
        }

        public ReadOnlySpan<byte> PayloadAsBytes()
        {
            if (MetaData.ContentEncoding == "gzip")
            {
                var stream = new GZipStream(new MemoryStream(PayloadStorage), CompressionMode.Decompress);
                var decomStream = new MemoryStream();
                stream.CopyTo(decomStream);
                var decompressedStorage = decomStream.ToArray();
                _PayloadSize = decompressedStorage.Length;
                return decompressedStorage;
            }
            _PayloadSize = PayloadStorage.Length;
            return PayloadStorage;
        }

        public string PayloadAsString()
        {
            return Encoding.UTF8.GetString(PayloadAsBytes());
        }

        public T PayloadAs<T>()
        {
            return JsonSerializer.Deserialize<T>(PayloadAsBytes());
        }
    }
}