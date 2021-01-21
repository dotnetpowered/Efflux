using System;
using System.IO;
using Efflux.FileSystem;

namespace Efflux.Samples.FileSystem
{
    public static class FileSystemSample
    {
        // Immutable File System
        public static void Demo(ITopic topic)
        {
            IFileSystem fileSystem = new ImmutableFileSystem(topic);

            var path = new FileSystemPath().AppendDirectory("opt").AppendDirectory(Guid.NewGuid().ToString()).AppendFile("temp.dat");
            using (var stream = fileSystem.CreateFile(path))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("Hello World");
                writer.WriteLine("This rocks");
            }

            Console.WriteLine($">> Read file from path {path}");
            using (var stream = fileSystem.OpenFile(path, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    Console.WriteLine(reader.ReadLine());
                }
            }
        }
    }
}
