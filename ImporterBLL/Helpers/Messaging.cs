using System;

namespace ImporterBLL.Helpers
{
    public class Messaging
    {

        /// <summary>
        /// Sends an email to a designated email address with details of the exception, the file it was trying to process and the service
        /// </summary>
        /// <param name="ex">The Exception</param>
        /// <param name="currentFilePath">The path to the file with the error in it</param>
        /// <param name="emailAddress">The email address to send to</param>
        /// <param name="serviceName">The name of the service that the exception/error was found in</param>
        public static void SendExceptionEmail(Exception ex, string currentFilePath, string emailAddress, string serviceName)
        {
            EmailHelper.SendException(
                ex,
                emailAddress,
                serviceName,
                "File: " + currentFilePath);
        }
        
        public static void SendErrorReportEmail(string body, string fromEmailAddress, string fromFriendlyName, string toEmailAddress, string serviceName)
        {
            EmailHelper.SendEmail(fromEmailAddress, fromFriendlyName, toEmailAddress, serviceName + " FAILED", body);
        }

        internal static void SendServiceReport(string fromEmailAddress, string fromFriendlyName, string toEmailAddress, string serviceName, string body, Constants.ProcessOutcome lastActionOutcome)
        {
            var subject = string.Format("{0} Service SUCCEEDED (or RECOVERED from failure) ({1})", serviceName, lastActionOutcome.ToString());

            if (lastActionOutcome == Constants.ProcessOutcome.ExpectedFileNotFoundError)
                subject = string.Format("{0} Service WARNING ({1})", serviceName, lastActionOutcome.ToString());
            else if(lastActionOutcome != Constants.ProcessOutcome.Success)
                subject = string.Format("{0} Service FAILED ({1})", serviceName, lastActionOutcome.ToString());

            EmailHelper.SendEmail(fromEmailAddress, fromFriendlyName, toEmailAddress, subject, body);
        }

    }
}
