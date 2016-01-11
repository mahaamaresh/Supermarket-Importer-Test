using System;

namespace ImporterBLL.Exceptions
{
    public class InvalidValidationException : Exception
    {
        public int RowNo { get; set; }
        public string Details { get; set; }
        public string Field { get; set; }
        public string Reason { get; set; }

        public InvalidValidationException(int rowNo, string field, string reason, string details)
            : base()
        {
            RowNo = rowNo;
            Details = details;
            Field = field;
            Reason = reason;
        }

        public override string Message
        {
            get { return string.Format("Invalid content. Failed at row: {0}, field: {1}, reason: {2}, content: '{3}'", RowNo, Field, Reason, Details); }
        }
    }
}
