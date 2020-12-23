using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Efflux.FileSystem
{
    public class ImmutableFileSystem : IFileSystem
    {
        ITopic topic;

        public ImmutableFileSystem(ITopic topic)
        {
            this.topic = topic;
        }

        public void Dispose()
        {
            
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            throw new NotImplementedException();
        }

        public bool Exists(FileSystemPath path)
        {
            var task = this.topic.FindByIdAsync(path.Path, false);
            task.Wait();
            return task.Result != null;
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            if (access != FileAccess.Read)
                throw new InvalidOperationException("This is an immuntable filesystem.");
            var task = this.topic.FindByIdAsync(path.Path);
            task.Wait();
            return new MemoryStream(task.Result.PayloadAsBytes().ToArray());
        }

        public Stream CreateFile(FileSystemPath path)
        {
            return new FileSystemWriteStream(topic, path);
        }

        public void CreateDirectory(FileSystemPath path)
        {
            throw new NotImplementedException();
        }

        public void Delete(FileSystemPath path)
        {
            throw new NotImplementedException();
        }
    }
}
