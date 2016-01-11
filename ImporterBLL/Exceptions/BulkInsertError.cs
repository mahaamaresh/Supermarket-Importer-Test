using System;

namespace ImporterBLL.Exceptions
{
    public class BulkInsertError : Exception
    {
        public BulkInsertError(string message) : base(message) { }
    }
}
