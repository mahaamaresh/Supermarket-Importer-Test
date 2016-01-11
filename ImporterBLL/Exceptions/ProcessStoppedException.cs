using System;

namespace ImporterBLL.Exceptions
{
    public class ProcessStoppedException : Exception
    {
        public ProcessStoppedException(string message) : base(message) { }
    }
}