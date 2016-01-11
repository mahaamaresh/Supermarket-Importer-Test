using System;

namespace ImporterBLL.Exceptions
{
    public class DeltaActionNotMatchException : Exception
    {
        public int RowNo { get; set; }
        public string RowContent { get; set; }
        public string ActionType { get; set; }
        public string Reason { get; set; }

        public DeltaActionNotMatchException(int rowno, string rowcontent, string actionType, string reason)
            : base()
        {
            RowNo = rowno;
            RowContent = rowcontent;
            ActionType = actionType;
            Reason = reason;
        }

        public override string Message
        {
            get { return string.Format("Insert/Update/Delete operation failed. Row: {0}, ActionType: {1}, Content: {2}, Reason: {3}", RowNo, ActionType, RowContent, Reason); }
        }
    }
}
