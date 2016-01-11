using System;

namespace ImporterBLL.Exceptions
{
    public class ArchiveError : Exception
    {
        public ArchiveError(string message) : base(message) { }
    }
}
