using System;
using System.IO;
using MimeMapping;

namespace Efflux.FileSystem
{
    public class FileSystemWriteStream : MemoryStream
    {
        readonly ITopic topic;
        readonly FileSystemPath path;
        bool isClosed;

        internal FileSystemWriteStream(ITopic topic, FileSystemPath path)
        {
            this.topic = topic;
            this.path = path;
        }

        public override void Close()
        {
            base.Close();
            if (!isClosed)
            {
                var contentType = MimeUtility.GetMimeMapping(path.EntityName);
                var message = new EffluxMessage(this.ToArray(), contentType);
                message.MetaData.Id = path.ToString();
                topic.WriteMessageAsync(message).Wait();
                isClosed = true;
            }
        }
    }
}
