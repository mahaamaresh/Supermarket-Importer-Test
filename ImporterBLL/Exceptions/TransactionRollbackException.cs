using System;

namespace ImporterBLL.Exceptions
{
    public class TransactionRollbackException : Exception
    {
        public string FileName { get; set; }
        public string Detail { get; set; }

        public TransactionRollbackException(string fileName, string message)
            : base()
        {
            FileName = fileName;
            Detail = message;
        }

        public override string Message
        {
            get { return string.Format("Transaction rollback. File: {0}, Message: {1}", FileName, Detail); }
        }
    }
}
