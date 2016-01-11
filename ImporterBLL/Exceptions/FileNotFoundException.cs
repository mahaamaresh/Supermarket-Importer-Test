using System;

namespace ImporterBLL.Exceptions
{
    public class FileNotFoundException : Exception
    {
        public string FileName { get; set; }

        public FileNotFoundException(string fileName)
            : base()
        {
            FileName = fileName;
        }

        public override string Message
        {
            get { return string.Format("{0} file not found", FileName); }
        }
    }
}
