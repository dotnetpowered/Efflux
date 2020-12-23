using System;

namespace Efflux.FileSystem
{
    // https://github.com/bobvanderlinden/sharpfilesystem/blob/master/SharpFileSystem/ParseException.cs
    public class ParseException : Exception
    {
        public string Input { get; private set; }
        public string Reason { get; private set; }

        public ParseException(string input)
            : base("Could not parse input \"" + input + "\"")
        {
            Input = input;
            Reason = null;
        }

        public ParseException(string input, string reason)
            : base("Could not parse input \"" + input + "\": " + reason)
        {
            Input = input;
            Reason = reason;
        }
    }
}