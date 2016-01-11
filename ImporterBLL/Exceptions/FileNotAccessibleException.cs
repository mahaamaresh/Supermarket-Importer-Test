using System;

namespace ImporterBLL.Exceptions
{
    public class FileNotAccessibleException : Exception
    {
        public string FileName { get; set; }
        public string Detail { get; set; }

        public FileNotAccessibleException(string fileName, string message)
            : base()
        {
            FileName = fileName;
            Detail = message;
        }

        public override string Message
        {
            get { return string.Format("Failed to open/read {0} file. Message: {1}", FileName, Detail); }
        }
    }
}
