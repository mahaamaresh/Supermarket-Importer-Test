using System;

namespace ImporterBLL.Exceptions
{
    public class EmptyFileException : Exception
    {
        public EmptyFileException(string message) : base(message) { }
    }
}
