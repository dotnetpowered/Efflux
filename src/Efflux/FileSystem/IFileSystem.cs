using System;
using System.Collections.Generic;
using System.IO;

namespace Efflux.FileSystem
{
    // Maybe switch to - https://github.com/SharpGrip/FileSystem ?
        
    // https://github.com/bobvanderlinden/sharpfilesystem/blob/master/SharpFileSystem/IFileSystem.cs
    public interface IFileSystem : IDisposable
    {
        ICollection<FileSystemPath> GetEntities(FileSystemPath path);
        bool Exists(FileSystemPath path);
        Stream CreateFile(FileSystemPath path);
        Stream OpenFile(FileSystemPath path, FileAccess access);
        void CreateDirectory(FileSystemPath path);
        void Delete(FileSystemPath path);
    }
}
