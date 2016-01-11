using System;

namespace ImporterBLL.Exceptions
{
    public class EndOfDayException : Exception
    {
        public override string Message
        {
            get
            {
                return "Importer has run past the end of the day, brut forcing it to stop. Please investigate why importer didn't complete when the file was present.";
            }
        }
    }
}
