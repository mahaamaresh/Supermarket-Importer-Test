using System;

namespace ImporterBLL.Exceptions
{
    public class InvalidFileFormatException : Exception
    {
        public int RowNo { get; set; }
        public string Details { get; set; }

        public InvalidFileFormatException(int rowNo, string details)
            : base()
        {
            RowNo = rowNo;
            Details = details;
        }

        public override string Message
        {
            get { return string.Format("Flat file not well-formed. Failed at row: {0}, content: '{1}'", RowNo, Details); }
        }
    }
}
