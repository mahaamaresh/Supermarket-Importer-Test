using System;

namespace ImporterBLL.Exceptions
{
    public class NoRecordFoundException : Exception
    {
        public string FileName { get; set; }

        public NoRecordFoundException(string fileName)
            : base()
        {
            FileName = fileName;
        }

        public override string Message
        {
            get { return string.Format("No {0} data found", FileName); }
        }
    }
}
