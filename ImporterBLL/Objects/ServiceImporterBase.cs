using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ImporterBLL.Exceptions;
using ImporterBLL.Helpers;
using ImporterBLL.Properties;
using WoolworthsDAL;

namespace ImporterBLL.Objects
{
    public abstract class ServiceImporterBase
    {
        protected enum LogType
        {
            Warning = 1,
            Log = 2,
            Error = 3
        }

        //Public properties
        public bool IgnoreFinalSleepCall { get; set; }
        public bool IgnoreStartTimeCheck { get; set; }
        public bool IgnoreHigherPriorityCheck { get; set; }

        protected string DaysToRunString { get; private set; }
        private List<DayOfWeek> _daysToRun;
        public List<DayOfWeek> DaysToRun
        {
            get
            {
                //if this is not set, assume process will run daily
                if (String.IsNullOrEmpty(DaysToRunString))
                {
                    _daysToRun = new List<DayOfWeek>()
                    {
                        DayOfWeek.Sunday,
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Thursday,
                        DayOfWeek.Friday,
                        DayOfWeek.Saturday
                    };
                }
                else
                {
                    _daysToRun = new List<DayOfWeek>();
                    foreach (var dayToRun in DaysToRunString.Split(','))
                    {
                        DayOfWeek day;
                        if (Enum.TryParse<DayOfWeek>(dayToRun, out day))
                        {
                            _daysToRun.Add(day);
                        }
                    }
                }

                return _daysToRun;
            }
        }

        //abstract settings to be overriden in derived classes
        protected abstract string ResetProcName { get; }
        protected abstract string DataProcessProcName { get; }

        //Settings determined by this class
        protected ServiceLogger.FlatFileType FlatFileType { get; private set; }
        protected int ServiceId { get { return (int)FlatFileType; } }
        protected int MasterLogId { get; private set; }
        protected FlatFiles.FileFormat FileFormat { get; private set; }
        protected FlatFiles.CompressType CompressionType { get; private set; }
        protected bool IsDatabaseRelated { get; private set; }
        protected DateTime StartOfDayUTCWhenCreated { get; private set; }
        public bool IsAdhocImport { get; set; }
        public bool AdhocForceProcess { get; set; }
        public bool AdhocStopSameProcessRunningToday { get; set; }

        //Settings related to the files being imported
        protected string FileDirectoryPath { get; private set; }
        protected string AdhocFileDirectoryPath { get; private set; }
        protected string ArchiveDirectoryPath { get; private set; }
        protected string SqlaServerPath { get; private set; }
        protected string SqlbServerPath { get; private set; }
        protected string SqlLocalPath { get; private set; }
        protected string TempUploadFolder { get; private set; }
        protected abstract List<string> FileNames { get; }
        protected List<string> AdhocFileNames { get; set; }
        protected string FileNamesForDb
        {
            get
            {
                if (FileNames != null && !IsAdhocImport)
                    return String.Join(",", FileNames.ToArray());
                else if (AdhocFileNames != null && IsAdhocImport)
                    return String.Join(",", AdhocFileNames.ToArray());
                else
                    return string.Empty;
            }
        }


        //Settings related to the bulk inserts and processing
        protected string StagingTableName { get; private set; }
        protected string FormatFilePath { get; private set; }
        protected virtual int? CommandTimeoutInSeconds
        {
            get { return Settings.Default.CommandTimeoutInSeconds; }
        }

        //Email settings
        protected string SummaryReportErrorToEmailAddress { get; private set; }
        protected string SummaryReportFromEmailAddress { get; private set; }
        protected string SummaryReportFromAddressFriendlyName { get; private set; }

        protected Constants.ProcessOutcome ProcessOutcome { get; set; }

        protected bool IsProcessSuccessful
        {
            get
            {
                return ProcessOutcome == Constants.ProcessOutcome.Success || ProcessOutcome == Constants.ProcessOutcome.AdhocSuccess;
            }
        }

        //Settings related to wait times
        protected virtual int? InitialWaitTimeInSeconds
        {
            get { return Settings.Default.InitialWaitTimeInSeconds > 300 ? 300 : Settings.Default.InitialWaitTimeInSeconds; }
        }

        protected virtual int? HigherProcessWaitTimeInSeconds
        {
            get { return Settings.Default.HigherProcessWaitTimeInSeconds > 300 ? 300 : Settings.Default.HigherProcessWaitTimeInSeconds; }
        }

        protected virtual int? FileWaitTimeInSeconds
        {
            get { return Settings.Default.FileWaitTimeInSeconds > 300 ? 300 : Settings.Default.FileWaitTimeInSeconds; }
        }

        protected virtual int? FinalWaitTimeInSeconds
        {
            //make file wait time 10 minutes max, as adhoc import won't be able to start if it's waiting for a file and sleeping.
            get { return Settings.Default.FinalWaitTimeInSeconds > 300 ? 300 : Settings.Default.FinalWaitTimeInSeconds; }
        }

        protected virtual TimeSpan? FinalTimeOfDayForFileExistCheck
        {
            get { return Settings.Default.FinalTimeOfDayForFileExistCheck; }
        }

        protected virtual TimeSpan? StartTime
        {
            get { return Settings.Default.StartTime; }
        }

        protected virtual int MaxAttemptsForDay
        {
            get { return Settings.Default.MaxAttemptsForDay; }
        }

        protected string DatabaseName
        {
            get { return Settings.Default.DatabaseName; }
        }

        protected bool IgnoreFileExistsError
        {
            get { return Settings.Default.IgnoreFileExistsError; }
        }

        protected List<string> FilePaths
        {
            get
            {
                var filePaths = new List<string>();
                if (IsAdhocImport)
                {
                    filePaths = (from f in AdhocFileNames
                                 select AdhocFileDirectoryPath + f).ToList();
                }
                else
                {
                    filePaths = (from f in FileNames
                                 select FileDirectoryPath + f).ToList();
                }
                return filePaths;
            }
        }

        protected List<string> UnzippedPaths;

        private bool _errorOnFileExistsCheck;

        protected bool AllowSameFileProcessing
        {
            get { return Settings.Default.AllowSameFileProcessing; }
        }

        protected ServiceImporterBase(ServiceLogger.FlatFileType flatFileType, FlatFiles.FileFormat fileFormat, FlatFiles.CompressType compressionType, string fileDirectoryPath, string archiveDirectoryPath, string stagingTableName, string formatFilePath,
            string summaryReportErrorToEmailAddress, string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, string sqlaServerPath, string sqlbServerPath, string localSqlPath, string temporaryUploadPath, 
            string daysToRun, bool isdatabaseRelated = true)
        {
            FlatFileType = flatFileType;
            FileFormat = fileFormat;
            CompressionType = compressionType;

            FileDirectoryPath = fileDirectoryPath;
            ArchiveDirectoryPath = archiveDirectoryPath;
            SqlaServerPath = sqlaServerPath;
            SqlbServerPath = sqlbServerPath;
            SqlLocalPath = localSqlPath;
            StagingTableName = stagingTableName;
            FormatFilePath = formatFilePath;
            SummaryReportErrorToEmailAddress = summaryReportErrorToEmailAddress;
            SummaryReportFromEmailAddress = summaryReportFromEmailAddress;
            SummaryReportFromAddressFriendlyName = summaryReportFromAddressFriendlyName;
            IsDatabaseRelated = isdatabaseRelated;
            TempUploadFolder = temporaryUploadPath;
            DaysToRunString = daysToRun;
        }

        protected void CheckProcessBeenForcedStopped()
        {
            //check importer hasn't been force stopped by adhoc importer
            //Logging.Log.Write(SeverityTypes.Verbose, "Start of CheckProcessBeenForcedStopped() function in ServiceImporterBase");
            var processForceStopped = false;
            using (var db = new WoolworthsDBDataContext())
            {
                var stoppedProcesses = (from i in db.ImportServiceMasters
                                        where i.Id == MasterLogId && i.ProcessManuallyStopped
                                        select i);
                processForceStopped = stoppedProcesses.Any();
            }

            // Logging.Log.Write(SeverityTypes.Verbose, "End of CheckProcessBeenForcedStopped() function in ServiceImporterBase");

            if (processForceStopped)
                throw new ProcessStoppedException("Process ending as it has been force stopped");
        }

        public Constants.ProcessOutcome Process()
        {
            //Master Check, continue sleeping if time is before StartTime
            //Master Check, if the file expected is already processed, no logging occurs, service will go back to sleep
            //Master Check, Max attempts for day
            CheckIsAdhocImport();
            if (!IsAdhocImport)
            {
                if (IsNotDayToRun() || IsBeforeStartTimeCheck() || (!ThisFileHasntBeenProcessed(false) && !AllowSameFileProcessing) || ExceededMaxAttempts())
                {
                    return ProcessOutcome;
                }
                //Create Master Log in DB
                CreateMasterLog();
            }

            try
            {
                ProcessInner();
            }
            catch (EndOfDayException ex)
            {
                ProcessOutcome = Constants.ProcessOutcome.EndOfDayHasPassedError;
                Log(LogType.Error, ex.Message);
                // Logging.Log.Write(SeverityTypes.Verbose, string.Format("End of day has passed error: {0}", ex.Message));
            }
            catch (BulkInsertError ex)
            {
                ProcessOutcome = Constants.ProcessOutcome.BulkInsertError;
                Log(LogType.Error, ex.Message);
                //  Logging.Log.Write(SeverityTypes.Verbose, string.Format("Bulk insert error: {0}", ex.Message));
            }
            catch (ArchiveError ex)
            {
                ProcessOutcome = Constants.ProcessOutcome.ArchiveError;
                Log(LogType.Error, ex.Message);
                //   Logging.Log.Write(SeverityTypes.Verbose, string.Format("Archive error: {0}", ex.Message));
            }
            catch (ProcessStoppedException ex)
            {
                ProcessOutcome = Constants.ProcessOutcome.ForcedStopped;
                Log(LogType.Error, ex.Message);
                //   Logging.Log.Write(SeverityTypes.Verbose, string.Format("Process stopped exception: {0}", ex.Message));
            }
            catch (TimeoutException ex)
            {
                UpdateMasterLogSuccess(false);
                // added a catch for timeouts here as this seems to be happenning a lot
                Log(LogType.Error, String.Format("Timeout exception in process: {0}. {1}", ex.Message, ex.StackTrace));
            }
            catch (Exception ex)
            {
                UpdateMasterLogSuccess(false);
                ProcessOutcome = Constants.ProcessOutcome.UnhandledExceptionError;
                Log(LogType.Error, String.Format("Unhandled exception in process: {0}. {1}", ex.Message, ex.StackTrace));
                //    Logging.Log.Write(SeverityTypes.Verbose, string.Format("Unhandled exception: {0}", ex.Message));
            }

            ProcessEnd();

            if (ProcessOutcome == Constants.ProcessOutcome.Success && IsAdhocImport)
                ProcessOutcome = Constants.ProcessOutcome.AdhocSuccess;

            return ProcessOutcome;
        }

        private void CheckIsAdhocImport()
        {

            //    Logging.Log.Write(SeverityTypes.Verbose, "Start of CheckIsAdhocImport() function in ServiceImporterBase");

            IsAdhocImport = false;
            AdhocForceProcess = false;
            AdhocStopSameProcessRunningToday = false;
            AdhocFileDirectoryPath = string.Empty;
            AdhocFileNames = null;

            var timeFromStartOfToday = DateTime.Now - DateTime.Now.Date;
            var utcDateMinusTimeFromStartOfToday = DateTime.UtcNow - timeFromStartOfToday;
            List<p_GetAdhocImportsResult> adhocImports;

            //   Logging.Log.Write(SeverityTypes.Verbose, "Querying DB for any adhoc imports in CheckIsAdhocImport()");

            using (var db = new WoolworthsDBDataContext())
            {
                adhocImports = db.p_GetAdhocImports(ServiceId).ToList();

            }


            if (adhocImports.Any())
            {
                //     Logging.Log.Write(SeverityTypes.Verbose, "Adhoc import found, flagging as IsAdhocImport=true in ServiceImporterBase");

                var adhocProcess = adhocImports.OrderBy(i => i.DateCreated).First();
                MasterLogId = adhocProcess.Id;
                IsAdhocImport = true;
                AdhocForceProcess = adhocProcess.AdhocForceProcess;
                AdhocStopSameProcessRunningToday = adhocProcess.AdhocStopSameProcessRunningToday;
                AdhocFileDirectoryPath = FileDirectoryPath;
                AdhocFileNames = string.IsNullOrEmpty(adhocProcess.FileNames) ? FileNames : adhocProcess.FileNames.Split(',').ToList();
                SetStartTimeOfService();
            }

        }

        private bool ExceededMaxAttempts()
        {
            //   Logging.Log.Write(SeverityTypes.Verbose, "Start of ExceededMaxAttempts() function in ServiceImporterBase");

            var timeFromStartOfToday = DateTime.Now - DateTime.Now.Date;
            var utcDateMinusTimeFromStartOfToday = DateTime.UtcNow - timeFromStartOfToday;
            var returnVal = false;
            using (var db = new WoolworthsDBDataContext())
            {
                returnVal = (from i in db.ImportServiceMasters
                             where i.ServiceId == ServiceId
                             && i.DateCreated > utcDateMinusTimeFromStartOfToday
                             && i.DateCreated < utcDateMinusTimeFromStartOfToday.AddDays(1)
                             && !i.CompletedSuccessful && !i.IsAdhocImport
                             select i).Count() >= MaxAttemptsForDay;
            }
            //  if (returnVal)
            //  {
            //      Log(LogType.Log, "Importer exceeded max attempts");
            //  }

            return returnVal;
        }

        private IList<ImportServiceMaster> ServiceEmailsSentToday
        {
            get
            {
                var timeFromStartOfToday = DateTime.Now - DateTime.Now.Date;
                var utcDateMinusTimeFromStartOfToday = DateTime.UtcNow - timeFromStartOfToday;
                using (var db = new WoolworthsDBDataContext())
                {
                    return (from i in db.ImportServiceMasters
                            where i.ServiceId == ServiceId
                            && i.FileNames == FileNamesForDb
                            && i.DateCreated > utcDateMinusTimeFromStartOfToday
                            && i.DateCreated < utcDateMinusTimeFromStartOfToday.AddDays(1)
                            && i.EmailSent
                            select i).ToList();
                }
            }
        }

        //this is a bit dirty, but without changing the base service class (tigerspikes ErrorHandledService), this is the simpliest way to handle a manual stop (with logging)
        public void ManualStop()
        {
            if (MasterLogId != 0)
            {
                var alreadyCompleted = Repository<ImportServiceMaster>.Single(m => m.Id == MasterLogId).DateCompleted != null;
                Repository<ImportServiceMaster>.Update(m => m.Id == MasterLogId, m =>
                {
                    if (!alreadyCompleted)
                    {
                        m.DateCompleted = DateTime.UtcNow;
                        m.CompletedSuccessful = false;
                    }

                    m.ProcessManuallyStopped = true;
                });
            }
        }

        private bool IsNotDayToRun()
        {
            //   Logging.Log.Write(SeverityTypes.Verbose, "Proccessing IsNotDayToRun() function in ServiceImporterBase");
            return !DaysToRun.Contains(DateTime.Now.DayOfWeek);
        }

        private bool IsBeforeStartTimeCheck()
        {
            //  Logging.Log.Write(SeverityTypes.Verbose, "Proccessing IsBeforeStartTimeCheck() function in ServiceImporterBase");
            //no need to check for unset value for StartTime, default of 00:00:00 will always return false
            if (IgnoreStartTimeCheck) return false;

            return DateTime.Now.TimeOfDay < StartTime;
        }

        private void ProcessEnd()
        {
            UpdateMasterLogSuccess(IsProcessSuccessful);

            Log(LogType.Log, String.Format("Process Complete - Result: {0} - Process will wake up every {1}", IsProcessSuccessful ? "Success" : "Failed",
                    Settings.Default.SleepPeriod));

            //Send report
            TrySendReport();
        }

        private void TrySendReport()
        {
            var sendEmailSuccess = false;
            var emailErrorMessage = String.Empty;

            //don't send if failed on exists error and ignore file exists error is set
            if (IgnoreFileExistsError && _errorOnFileExistsCheck && !IsAdhocImport)
            {
                Log(LogType.Log, "Email not sent as service failed on FileExistsError but IgnoreFileExistsError setting is set to true.");
                return;
            }

            try
            {
                var ops = new DataLoadOptions();
                ops.LoadWith<ImportServiceMaster>(i => i.ImportServiceLogs);
                var master = Repository<ImportServiceMaster>.Single(i => i.Id == MasterLogId, ops);
                var serviceEmailsSentToday = ServiceEmailsSentToday;
                var successEmailSentToday = serviceEmailsSentToday.Any(s => s.EmailSent && s.ProcessOutcome.HasValue && s.ProcessOutcome == (int)Constants.ProcessOutcome.Success);
                var failureEmailSentToday = serviceEmailsSentToday.Any(s => s.EmailSent && s.ProcessOutcome.HasValue && s.ProcessOutcome != (int)Constants.ProcessOutcome.Success);
                var sameOutcomeEmailSentToday = serviceEmailsSentToday.Any(s => s.EmailSent && s.ProcessOutcome.HasValue && s.ProcessOutcome == (int)ProcessOutcome);
                var serviceHasRecovered = IsProcessSuccessful && !successEmailSentToday && failureEmailSentToday;

                if (IsAdhocImport || !IsProcessSuccessful || !Settings.Default.EmailServiceReportOnErrorOnly || serviceHasRecovered)
                {
                    var sendEmailNow = (!IsProcessSuccessful && !sameOutcomeEmailSentToday) || serviceHasRecovered || IsAdhocImport;
                    if (sendEmailNow)
                    {
                        var body = CreateEmailBody(master);
                        Messaging.SendServiceReport(SummaryReportFromEmailAddress, SummaryReportFromAddressFriendlyName, SummaryReportErrorToEmailAddress, FlatFileType.ToString(), body, ProcessOutcome);
                        sendEmailSuccess = true;
                        Repository<ImportServiceMaster>.Update(m => m.Id == MasterLogId, m =>
                        {
                            m.DateCompleted = DateTime.UtcNow;
                            m.EmailSent = true;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                emailErrorMessage = ex.Message;
            }

            if (!String.IsNullOrWhiteSpace(emailErrorMessage)) Log(LogType.Warning, String.Format("Email Report NOT sent, exception message: {0}", emailErrorMessage));
            else if (sendEmailSuccess) Log(LogType.Log, "Email Report sent successfully");
            else if (!IsProcessSuccessful) Log(LogType.Log, "Email report not sent as service will retry until MaxAttempts reached before sending error email");
            else Log(LogType.Log, "Email not sent as service completed successfully");


        }

        private string CreateEmailLogSummary(EntitySet<ImportServiceLog> importServiceLogs)
        {
            var sb = new StringBuilder();

            foreach (var log in importServiceLogs.Where(l => l.MasterServiceId == MasterLogId))
            {
                var logTypeIsError = (LogType)log.LogType == LogType.Error;
                sb.Append("<p>");
                if (logTypeIsError)
                    sb.Append("<strong><u>");
                sb.Append(log.DateCreated.ToLocalTime().ToString());
                sb.Append(" - ");
                sb.Append(((LogType)log.LogType).ToString());
                sb.Append(" - ");
                sb.Append(log.Description);
                if (logTypeIsError)
                    sb.Append("</u></strong>");
                sb.Append("</p>");
            }

            return sb.ToString();
        }

        private string CreateFileErrorEmailBody(ImportServiceMaster master)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body>");
            sb.Append("<h1>");
            sb.Append(FlatFileType.ToString());
            sb.Append(" Service Report -- File Missing ERROR");
            sb.Append("</h1>");
            sb.Append("<br />");
            if (IsAdhocImport)
                sb.Append("<strong><u>ACTION:</u> Please check file. Expected file for adhoc import was not found, please re-run adhoc import with updated file name.</strong>");
            else
                sb.Append("<strong><u>ACTION:</u> No action needed from TS, awaiting file to be copied to FTP directory by WoW</strong>");
            sb.Append("<br /><br />");
            sb.AppendFormat("<p>Service with Id: {0}, for file(s): {1}{2}</p>", master.Id, master.FilePath, master.FileNames);
            sb.AppendFormat("<p>File check ERROR, see log. <strong><u>This process will continue to check files every {0} seconds, until {1}</u></strong>", FileWaitTimeInSeconds.ToString(), FinalTimeOfDayForFileExistCheck.Value.ToString());
            sb.Append("<p>Log:</p>");
            sb.Append(CreateEmailLogSummary(master.ImportServiceLogs));
            sb.Append("</body></html>");

            return sb.ToString();
        }

        private string CreateEmailBody(ImportServiceMaster master)
        {
            var outcome = IsProcessSuccessful ? "SUCCESS!" : ProcessOutcome == Constants.ProcessOutcome.ForcedStopped ? "STOPPED" : "FAILED!";
            var sb = new StringBuilder();
            sb.Append("<html><body>");
            sb.Append("<h1>");
            sb.Append(FlatFileType.ToString());
            sb.Append(" Service Report -- ");
            sb.Append(outcome);
            sb.Append("</h1>");
            sb.Append("<br />");
            if (ProcessOutcome == Constants.ProcessOutcome.ForcedStopped)
            {
                sb.AppendFormat("<strong>Outcome type: <u>{0}</u></strong>", ProcessOutcome.ToString());
                sb.AppendFormat("<br /><br /><strong><U>ACTION:</u> None needed, as this process was force stopped by an adhoc import that has been run. Please visit <a href='{0}' target='_blank'>Wiki page</a> for more detailed information.</strong>", Settings.Default.WikiHelpPageURL);

            }
            else if (!IsProcessSuccessful)
            {
                sb.AppendFormat("<strong>Error type: <u>{0}</u></strong>", ProcessOutcome.ToString());
                sb.AppendFormat("<br /><br /><strong><U>ACTION:</u> Please visit <a href='{0}' target='_blank'>Wiki page</a> for specific error information.</strong>", Settings.Default.WikiHelpPageURL);
            }
            sb.Append("<br /><br />");
            sb.AppendFormat("<p>Service with Id: {0}, for file(s): {1}  {2}</p>", master.Id, master.FileNames, IsProcessSuccessful ? "SUCCEEDED" : "FAILED");
            sb.AppendFormat("<p>Service started: {0}, completed: {1}</p>", master.DateCreated.ToLocalTime().ToString(), master.DateCompleted.Value.ToLocalTime().ToString());
            sb.Append("<p>Log:</p>");
            sb.Append(CreateEmailLogSummary(master.ImportServiceLogs));
            sb.Append("</body></html>");

            return sb.ToString();
        }

        private void ProcessInner()
        {
            //  Logging.Log.Write(SeverityTypes.Verbose, "Start of ProcessInner() function in ServiceImporterBase");
            //Validate properties - Log on null or empty
            string properties;
            if (!ValidateProperties(out properties))
            {
                //Log ERROR AND END
                ProcessOutcome = Constants.ProcessOutcome.ValidatePropertyError;
                Log(LogType.Error, String.Format("ValidateProperties method returned false for these properties that are not set correctly in configuration: {0}", properties));
                return;
            }

            //update masterlog
            UpdateMasterLogFileDetails(FileDirectoryPath, FileNames);

            if (!IsAdhocImport || !AdhocForceProcess)
            {
                //Wait InitialWakeUpWaitTime (let other processes wake up and log)
                var message = String.Format("Initial Wait for {0} seconds, to allow highest priority service to run", InitialWaitTimeInSeconds.ToString());
                Log(LogType.Log, message);
                //   Logging.Log.Write(SeverityTypes.Verbose, message);
                Sleep(InitialWaitTimeInSeconds.Value * 1);
            }

            //Log file Checking
            Log(LogType.Log, "Checking file status");
            var errorEmailSent = false;
            string fileReason;
            if (!IsNewFileValidAndExists(out fileReason))
            {
                string oldFileReason = null;
                do
                {
                    CheckProcessBeenForcedStopped();
                    if (oldFileReason != fileReason) //will always log the first time, if the reason changes will log again
                    {   //Log LOG - waiting for file to exist
                        ProcessOutcome = Constants.ProcessOutcome.ExpectedFileNotFoundError;
                        Log(LogType.Log, String.Format("Expected file not found in FTP directory from WoW. Reason: {0} - Process will wake up every {1} seconds to attempt to find file again", fileReason, FileWaitTimeInSeconds.ToString()));
                        Repository<ImportServiceMaster>.Update(m => m.Id == MasterLogId, m => m.WaitReason = (int)Constants.WaitReason.FileDoesNotExist);

                        if (!errorEmailSent && !IsAdhocImport)
                        {
                            TrySendFileErrorEmail();
                            errorEmailSent = true;
                        }
                    }

                    if (IsAdhocImport)
                    {
                        ProcessOutcome = Constants.ProcessOutcome.ExpectedFileNotFoundError;
                        Log(LogType.Error, "Please check file. Expected file for adhoc import was not found, please re-run adhoc import with correct file name.");
                        return;
                    }
                    else if (DateTime.Now.TimeOfDay > FinalTimeOfDayForFileExistCheck)
                    {
                        //Log ERROR and END
                        ProcessOutcome = Constants.ProcessOutcome.TimeOfDayExceededError;
                        Log(LogType.Error, String.Format("Expected file still not found in FTP directory from WoW and importer has passed latest time it is scheduled to run (\"{0}\") from FinalTimeOfDayForFileExistCheck config setting", FinalTimeOfDayForFileExistCheck.ToString()));
                        return;
                    }
                    oldFileReason = fileReason;

                    //   Logging.Log.Write(SeverityTypes.Verbose, string.Format("Sleeping for {0} seconds waiting for file in ServiceImporterBase", FileWaitTimeInSeconds.Value));

                    Sleep(FileWaitTimeInSeconds.Value * 1000);
                } while (!IsNewFileValidAndExists(out fileReason));

                //  Logging.Log.Write(SeverityTypes.Verbose, "Stopped sleeping, file present in ServiceImporterBase");

                Repository<ImportServiceMaster>.Update(m => m.Id == MasterLogId, m => m.WaitReason = (int)Constants.WaitReason.None);
            }
            else
                CheckProcessBeenForcedStopped();


            //log LOG unzip file (this is logged in the method)
            string unzipError;
            if (!UnzipFileSuccessful(out unzipError))
            {
                //log ERROR and END
                ProcessOutcome = Constants.ProcessOutcome.UnzipError;
                Log(LogType.Error, unzipError);
                return;
            }

            //Log check against other processes
            string waitReason;
            if (IsWaitingForOtherProcessesToComplete(out waitReason))
            {
                string oldWaitReason = null;
                do
                {
                    if (oldWaitReason != waitReason) //will always log the first time, if the reason changes will log again
                    {
                        //Log LOG - waiting for higher process to complete, this will only log once, but will check continuously
                        Log(LogType.Log, String.Format("Waiting for higher priority process to complete. Reason: {0} - Will wake up every {1} seconds", waitReason, HigherProcessWaitTimeInSeconds.ToString()));
                    }
                    oldWaitReason = waitReason;
                    Sleep(HigherProcessWaitTimeInSeconds.Value * 1000);
                } while (IsWaitingForOtherProcessesToComplete(out waitReason));
            }

            CheckProcessBeenForcedStopped();
            //past the point of no return
            UpdateMasterLogProcessStarted();

            //log LOG run reset proc
            Log(LogType.Log, "Executing Reset Proc: " + ResetProcName);
            // Logging.Log.Write(SeverityTypes.Verbose, "Execute ExecuteResetProc() function in ServiceImporterBase");
            ExecuteResetProc();

            foreach (var path in UnzippedPaths)
            {
                if (IsDatabaseRelated)
                {
                    //log LOG bulk inserting
                    Log(LogType.Log, String.Format("Executing Bulk Insert Proc for path {0}", path));
                    var rowsinserted = ExecuteBulkInsert(path);
                    Log(LogType.Log, String.Format("{0} rows bulk inserted into staging table", rowsinserted));
                }
            }

            //  Logging.Log.Write(SeverityTypes.Verbose, "Execute ProcessBespoke() function in ServiceImporterBase");
            ProcessBespoke();

            //log LOG data processing
            Log(LogType.Log, "Executing Data Process Proc: " + DataProcessProcName);
            //    Logging.Log.Write(SeverityTypes.Verbose, "Execute ExecuteDataProcessingProc() function in ServiceImporterBase");
            if (!ExecuteDataProcessingProc())
            {
                ProcessOutcome = Constants.ProcessOutcome.HandledSQLExceptionError;
                //Error should be logged in proc
                return;
            }

            //    Logging.Log.Write(SeverityTypes.Verbose, "Execute AfterDataProcessed() function in ServiceImporterBase");
            AfterDataProcessed();

            //TC: want to update CompletedSuccessful flag to true so other importers can run as archiving step can be time consuming.
            //  Logging.Log.Write(SeverityTypes.Verbose, "Execute UpdateMasterLogSuccess() function in ServiceImporterBase");
            UpdateMasterLogSuccess(true);

            //Archive file
            //  Logging.Log.Write(SeverityTypes.Verbose, "Execute ArchiveFiles() function in ServiceImporterBase");
            ArchiveFiles();

            ProcessOutcome = Constants.ProcessOutcome.Success;
            //   Logging.Log.Write(SeverityTypes.Verbose, string.Format("Returning from ProcessInner, proces outcome is {0}", ProcessOutcome.ToString()));
            return;
        }

        /// <summary>
        /// Archive files up to S3
        /// </summary>
        private void ArchiveFiles()
        {
            try
            {
                Log(LogType.Log, "Archiving Files");
                foreach (var filePath in UnzippedPaths)
                {
                    FlatFiles.Archive(filePath, ArchiveDirectoryPath, true, FileFormat, TempUploadFolder);
                }
                //Move original Zips
                foreach (var originalPath in FilePaths)
                {
                    using (var fileStream = FlatFiles.GetS3File(originalPath))
                    {
                        if (fileStream != null)
                        {
                            var destFileName = String.Format("{0}{1}{2}",
                            Path.GetFileNameWithoutExtension(originalPath), "_original",
                            Path.GetExtension(originalPath));
                            FlatFiles.CopyFileToS3Archive(ArchiveDirectoryPath, fileStream, destFileName, TempUploadFolder);
                        }
                    }
                    // delete the now moved file
                    FlatFiles.DeleteS3File(originalPath);
                }
            }
            catch (Exception ex)
            {
                throw new ArchiveError("Error archiving files. Details: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        protected virtual void ProcessBespoke() { }

        private void TrySendFileErrorEmail()
        {
            //  Logging.Log.Write(SeverityTypes.Verbose, "Start of TrySendFileErrorEmail() function in ServiceImporterBase");

            //Only send if setting is set to send
            if (!Settings.Default.SendEmailOnInitialFileError && !IsAdhocImport) return;
            //Don't sent if the IgnoreFileExists error is true and the Service failed on the FileExistsCheck
            if (_errorOnFileExistsCheck && IgnoreFileExistsError && !IsAdhocImport) return;
            
            var sendEmailSuccess = false;
            var emailErrorMessage = String.Empty;

            try
            {
                var ops = new DataLoadOptions();
                ops.LoadWith<ImportServiceMaster>(i => i.ImportServiceLogs);
                var master = Repository<ImportServiceMaster>.Single(i => i.Id == MasterLogId, ops);

                var body = CreateFileErrorEmailBody(master);
                Messaging.SendServiceReport(SummaryReportFromEmailAddress, SummaryReportFromAddressFriendlyName, SummaryReportErrorToEmailAddress, FlatFileType.ToString(), body, ProcessOutcome);
                sendEmailSuccess = true;

                Repository<ImportServiceMaster>.Update(m => m.Id == MasterLogId, m =>
                {
                    m.DateCompleted = DateTime.UtcNow;
                    m.EmailSent = true;
                });
            }
            catch (Exception ex)
            {
                emailErrorMessage = ex.Message;
            }

            if (sendEmailSuccess)
            {
                Log(LogType.Log, "Initial File Error Email sent");
            }
            else if (!String.IsNullOrEmpty(emailErrorMessage))
            {
                Log(LogType.Warning, String.Format("Initial File Erorr message not sent because of exception: {0}", emailErrorMessage));
            }
            //    Logging.Log.Write(SeverityTypes.Verbose, "End of TrySendFileErrorEmail() function in ServiceImporterBase");
        }

        /// <summary>
        /// Method called after the ExecuteDataProcessingProc
        /// </summary>
        protected virtual void AfterDataProcessed() { }

        //Not sure about this, bascially this is copies the Sleep method used in the services, with the intention of being overrided if not used in a service.
        //Setting it here means that the "service" cannot do the final sleep, it has to be handled by the default sleep timer.
        protected virtual void Sleep(int SleepTimeInMilliseconds)
        {
            Thread.Sleep(SleepTimeInMilliseconds);
        }

        protected abstract bool ExecuteDataProcessingProc();

        /// <summary>
        /// gets the files from s3, copies them to the sql servers and runs a bulk insert on the 
        /// database
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual int ExecuteBulkInsert(string path)
        {

            // get the flat file and config from s3
            using (var flatFileStream = FlatFiles.GetS3File(path))
            using (var configStream = FlatFiles.GetS3File(FormatFilePath))
            {
                // run the insert on the Db, the files that will be used will depend on which server is currently the master
                int? rowsInserted = null;
                var flatFilePath = string.Empty;
                var configPath = string.Empty;
                try
                {
                    flatFilePath = Path.GetFileName(path);
                    configPath = Path.GetFileName(FormatFilePath);
                    // local version of files
                    var localFlatFilePath = SqlLocalPath + flatFilePath;
                    var localConfigPath = SqlLocalPath + configPath;

                    // copy these files to a local directory for both sql servers (in case of failover)
                    Log(LogType.Log, String.Format("Bulk Insert - Copying files to  {0}", SqlaServerPath));
                    try
                    {
                        CopyFileToServerPath(SqlaServerPath + flatFilePath, flatFileStream);
                        CopyFileToServerPath(SqlaServerPath + configPath, configStream);
                        //CopyFileToServerPath(SqlLocalPath + flatFilePath, flatFileStream);//xrvng
                        //CopyFileToServerPath(SqlLocalPath + configPath, configStream);//xrvng
                    }
                    catch (Exception ex)
                    {
                        Log(LogType.Warning, String.Format("Bulk Insert - Couldn't copy file to  {0}. ({1})Error: {2}", SqlaServerPath + flatFilePath + " " + SqlaServerPath + configPath, SqlaServerPath, ex.Message));
                    }

                    Log(LogType.Log, String.Format("Bulk Insert - Copying files to  {0}", SqlbServerPath));
                    try
                    {
                        CopyFileToServerPath(SqlbServerPath + flatFilePath, flatFileStream);
                        CopyFileToServerPath(SqlbServerPath + configPath, configStream);
                    }
                    catch (Exception ex)
                    {
                        Log(LogType.Warning, String.Format("Bulk Insert - Couldn't copy file to  {0}. ({1})Error: {2}", SqlbServerPath + flatFilePath + " " + SqlbServerPath + configPath, SqlbServerPath, ex.Message));
                    }


                    Log(LogType.Log, String.Format("Bulk Insert - using files:  {0} ,  {1}", localFlatFilePath, localConfigPath));

                    using (var db = new WoolworthsDBDataContext())
                    {
                        db.CommandTimeout = CommandTimeoutInSeconds.Value;
                        db.p_Import_DynamicBulk(StagingTableName, localFlatFilePath, localConfigPath, ref rowsInserted);
                    }
                }
                catch (Exception ex)
                {
                    throw new BulkInsertError(ex.Message);
                }
                finally
                {
                    // clean up the files from the sql servers
                    Log(LogType.Log, String.Format("Bulk Insert - Deleting files from {0}", SqlaServerPath));
                    if (File.Exists(SqlaServerPath + flatFilePath))
                        File.Delete(SqlaServerPath + flatFilePath);
                    if (File.Exists(SqlaServerPath + configPath))
                        File.Delete(SqlaServerPath + configPath);
                    Log(LogType.Log, String.Format("Bulk Insert - Deleting files from {0}", SqlbServerPath));
                    if (File.Exists(SqlbServerPath + flatFilePath))
                        File.Delete(SqlbServerPath + flatFilePath);
                    if (File.Exists(SqlbServerPath + configPath))
                        File.Delete(SqlbServerPath + configPath);
                }



                return rowsInserted.HasValue ? rowsInserted.Value : 0;
            }
        }

        /// <summary>
        /// copies files to a folder 
        /// </summary>
        /// <param name="sqlFilePath"></param>
        /// <param name="flatFileStream"></param>
        private static void CopyFileToServerPath(string sqlFilePath, Stream flatFileStream)
        {

            using (var flatFileLocal = File.Create(sqlFilePath))
            {
                flatFileStream.CopyTo(flatFileLocal);
            }
        }


        protected abstract void ExecuteResetProc();

        private bool UnzipFileSuccessful(out string unzipError)
        {
            //   Logging.Log.Write(SeverityTypes.Verbose, "Start of UnzipFileSuccessful() function in ServiceImporterBase");

            unzipError = null;
            UnzippedPaths = new List<string>();
            foreach (var path in FilePaths)
            {
                var unzipPath = Path.GetDirectoryName(path);

                //Log
                Log(LogType.Log, String.Format("Unzipping file at {0} to {1}", path, unzipPath));

                try
                {
                    // the images importer is special, that's right
                    if (FlatFileType.Equals(ServiceLogger.FlatFileType.Images))
                    {
                        // move them to the unzip path/unzipped
                        UnzipImages(path);
                    }
                    else
                    {
                        //will need consideration
                        ZipHelper.UnzipFile(path, unzipPath, CompressionType, TempUploadFolder);

                    }
                }
                catch (EmptyFileException ex)
                {
                    ProcessOutcome = Constants.ProcessOutcome.Success;
                    unzipError = String.Format("Error unzipping file: {0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace);

                    // archive file and end
                    ArchiveFiles();


                    return false;
                }
                catch (Exception ex)
                {
                    ProcessOutcome = Constants.ProcessOutcome.UnzipError;
                    unzipError = String.Format("Error unzipping file: {0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace);
                    return false;
                }

                // don't bother with this for images, given that in the zips are just directories
                if (!FlatFileType.Equals(ServiceLogger.FlatFileType.Images))
                {
                    //bascially this takes into account zipfiles where they have set the zip file to "SomethingZipped.txt.zip"
                    var unzipPathFull = Path.ChangeExtension(path, FileFormat.ToString());
                    if (unzipPathFull.EndsWith(String.Format(".{0}.{1}", FileFormat.ToString(), FileFormat.ToString()), true, CultureInfo.CurrentCulture))
                    {
                        unzipPathFull = unzipPathFull.Substring(0, unzipPathFull.LastIndexOf("."));
                    }

                    if (!FlatFiles.S3FileExists(unzipPathFull))
                    {
                        //check lowercase file ext too
                        unzipPathFull = Path.ChangeExtension(path, FileFormat.ToString().ToLower());
                        if (!FlatFiles.S3FileExists(unzipPathFull))
                        {
                            ProcessOutcome = Constants.ProcessOutcome.UnzipError;
                            unzipError = String.Format(
                                "Unzip returned successful but file does not exist at path: {0}", unzipPathFull);
                            return false;
                        }
                    }

                    UnzippedPaths.Add(unzipPathFull);
                }
            }

            //  Logging.Log.Write(SeverityTypes.Verbose, "End of UnzipFileSuccessful() function in ServiceImporterBase");

            return true;
        }

        protected virtual void UnzipImages(string path) { }

        private void UpdateMasterLogFileDetails(string FileDirectoryPath, List<string> FileNames)
        {
            //  Logging.Log.Write(SeverityTypes.Verbose, "Start of UpdateMasterLogFileDetails() function in ServiceImporterBase");

            Repository<ImportServiceMaster>.Update(m => m.Id == MasterLogId, m =>
            {
                m.FilePath = FileDirectoryPath;
                m.FileNames = FileNamesForDb;
            });

            // Logging.Log.Write(SeverityTypes.Verbose, "Start of UpdateMasterLogFileDetails() function in ServiceImporterBase");
        }

        private void UpdateMasterLogSuccess(bool success)
        {
            using (var db = new WoolworthsDBDataContext())
            {
                db.p_Import_Finished(MasterLogId, success, (int)ProcessOutcome);
            }
        }

        private void UpdateMasterLogProcessStarted()
        {
            //   Logging.Log.Write(SeverityTypes.Verbose, "Start of UpdateMasterLogProcessStarted() function in ServiceImporterBase");
            Repository<ImportServiceMaster>.Update(m => m.Id == MasterLogId, m => m.MainProcessStarted = true);
            //   Logging.Log.Write(SeverityTypes.Verbose, "End of UpdateMasterLogProcessStarted() function in ServiceImporterBase");
        }

        private bool IsWaitingForOtherProcessesToComplete(out string waitReason)
        {
            /* JUST A NOTE:
             * When I write "higher priority" in comments or variables I actually mean services which have a priority that is LESS THAN the priority
             * of this Process.
             * ie, a process with Priority 1 is of HIGHER PRIORITY than a process with priority 2
             */

            //Services run via priority, but as a precaution, the MainProcessStarted Flag is set when a service passes beyond this point
            //If this is the case, it's safer to let this service finish before running any other.
            //Update: Since priorities can now have the same value, we must cater for two services with the same priority attempting to run at the same time.
            //---- Fix to this is to go by "ID" if they have the same priority




            CheckProcessBeenForcedStopped();

            waitReason = null;

            var timeFromStartOfToday = DateTime.Now - DateTime.Now.Date;
            var utcDateMinusTimeFromStartOfToday = DateTime.UtcNow - timeFromStartOfToday;
            var timeSinceStartOfDayWhenStarted = utcDateMinusTimeFromStartOfToday - StartOfDayUTCWhenCreated;

            var ops = new DataLoadOptions();
            ops.LoadWith<ImportServiceMaster>(i => i.Service);
            var incompleteOperations = Repository<ImportServiceMaster>.Where(m => m.DateCompleted == null && m.DateCreated > utcDateMinusTimeFromStartOfToday, ops).ToList();

            //Gets the priority of this operation
            var priority = incompleteOperations.Single(i => i.Id == MasterLogId).Service.Priority;

            //Updated, services must now ALWAYS wait for processes with higher priorities to run FOR THIS DAY
            if (!IsAdhocImport || !AdhocForceProcess)
            {
                if (!IgnoreHigherPriorityCheck)
                {
                    if (timeSinceStartOfDayWhenStarted.Days >= 1)
                        throw new EndOfDayException();

                    //ignore this if service has priority 1 (nothing can have a higher priority)
                    if (priority != 1)
                    {
                        //Annoying code to get around the stored UTC dates interferring with what is considered "today"

                        var allTodayOperations = Repository<ImportServiceMaster>
                            .Where(m => m.DateCreated > utcDateMinusTimeFromStartOfToday
                                && m.DateCreated < utcDateMinusTimeFromStartOfToday.AddDays(1)
                                && m.CompletedSuccessful
                                , ops);
                        // var lowerPriorities = allTodayOperations.Select(a => a.Service.Priority).ToList();
                        var lowerPriorities = allTodayOperations.Where(a => (a.Service.Priority < priority));
                        // for (var lowerPriority = priority - 1; lowerPriority > 0; lowerPriority--)
                        //  {
                        //  if (!lowerPriorities.Contains(lowerPriority))
                        if (!lowerPriorities.Any())
                        {
                            var ids = string.Join(",", lowerPriorities.Select(a => a.Service.ID).ToArray());
                            waitReason = String.Format("Process waiting for higher priority processes to run for TODAY, this service priority is: {0}. Id is/are {1}", priority, ids);
                            return true;
                        }
                        //}
                    }


                    //sets to true if an operation exists that has a lower priority

                    var incompleteHigherPriority = incompleteOperations.Where(i => i.Service.Priority < priority);
                    if (incompleteHigherPriority.Any())
                    {
                        var ids = string.Join(",", incompleteHigherPriority.Select(a => a.Service.ID).ToArray());
                        waitReason = String.Format("Process waiting for higher priority processes to run that are INCOMPLETE, this service priority is: {0}. Id is/are {1}", priority, ids);
                        return true;
                    }

                    var equalServices = (from i in incompleteOperations
                                         where i.Service.Priority <= priority
                                         && i.Id < MasterLogId
                                         orderby i.Id
                                         select new { i.WaitReason, i.Id });

                    //if all prior equal services have a wait reason of type FileDoesNotExist, then don't wait for these services.
                    if (equalServices.Any(s => !s.WaitReason.HasValue ||
                                            (s.WaitReason.HasValue && (Constants.WaitReason)s.WaitReason.Value != Constants.WaitReason.FileDoesNotExist)))
                    {
                        var ids = string.Join(",", equalServices.Select(a => a.Id).ToArray());
                        waitReason = String.Format("Process waiting as another processes of the same priority exists, and was created before this. Id is/are {0}", ids);
                        return true;
                    }
                }
            }

            if (incompleteOperations.Any(i => i.MainProcessStarted))
            {
                waitReason = "Process waiting as an incomplete process has passed the MainProcessStarted point";
                //return true; xrvng for testing
            }


            return false;
        }

        private bool ValidateProperties(out string invalidProperties)
        {
            //Logging.Log.Write(SeverityTypes.Verbose, "Start of ValidateProperties() function in ServiceImporterBase");

            var sb = new StringBuilder();

            StringCheck(FileDirectoryPath, "FileDirectoryPath", sb);
            StringCheck(SummaryReportErrorToEmailAddress, "SummaryReportEmailAddress", sb);

            if (IsDatabaseRelated)
            {
                StringCheck(StagingTableName, "StagingTableName", sb);
                StringCheck(FormatFilePath, "FormatFilePath", sb);
                StringCheck(DatabaseName, "DatabaseName", sb);
            }

            if (FileNames == null || FileNames.Count == 0)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append("FileNames");
            }

            NullableTypeCheck(InitialWaitTimeInSeconds, "InitialWaitTimeInSeconds", sb);
            NullableTypeCheck(InitialWaitTimeInSeconds, "InitialWaitTimeInSeconds", sb);
            NullableTypeCheck(FinalWaitTimeInSeconds, "FinalWaitTimeInSeconds", sb);
            NullableTypeCheck(HigherProcessWaitTimeInSeconds, "WaitingForHigherProcessWaitTimeInSeconds", sb);
            NullableTypeCheck(FinalTimeOfDayForFileExistCheck, "FinalTimeOfDayForFileExistCheck", sb);
            NullableTypeCheck(FileWaitTimeInSeconds, "WaitForFileWaitTimeInSeconds", sb);
            NullableTypeCheck(CommandTimeoutInSeconds, "CommandTimeout", sb);

            if (DaysToRun == null || DaysToRun.Count == 0)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append("DaysToRun");
            }

            invalidProperties = sb.ToString();

            //    Logging.Log.Write(SeverityTypes.Verbose, "End of ValidateProperties() function in ServiceImporterBase");

            return sb.Length == 0;
        }

        private void StringCheck(string stringToCheck, string propertyName, StringBuilder sb)
        {
            if (String.IsNullOrWhiteSpace(stringToCheck))
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(propertyName);
            }
        }

        private void NullableTypeCheck<T>(Nullable<T> nullableType, string propertyName, StringBuilder sb) where T : struct
        {
            if (!nullableType.HasValue)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(propertyName);
            }
        }

        private bool IsNewFileValidAndExists(out string waitReason)
        {
            //         Logging.Log.Write(SeverityTypes.Verbose, "Processing IsNewFileValidAndExists() function in ServiceImporterBase");

            waitReason = null;
            //There used to be a regex file check here, it was pointless so I removed it.

            if (!IsAdhocImport && (!ThisFileHasntBeenProcessed(true) && !AllowSameFileProcessing))
            {
                //log WARNING -- maybe this should be an error?
                waitReason = "File version is the same as previous version";
                return false;
            }

            if (!CheckFileExists())
            {
                //log WARNING
                _errorOnFileExistsCheck = true;
                waitReason = "File at file path specified does not exist";
                return false;
            }
            _errorOnFileExistsCheck = false;

            /*
             * not relevant for s3
            if (!CheckFileCanBeRead())
            {
                //log WARNING
                waitReason = "File at file path specified cannot be read";
                return false;
            }
             * */

            return true;
        }

        private bool ThisFileHasntBeenProcessed(bool checkWithMasterId)
        {
            //        Logging.Log.Write(SeverityTypes.Verbose, "Start of ThisFileHasntBeenProcessed() function in ServiceImporterBase");

            ImportServiceMaster previousMaster;
            using (var db = new WoolworthsDBDataContext())
            {
                //Builds the query, this won't execute until FirstOrDefault is called
                var q = from m in db.ImportServiceMasters
                        where m.ServiceId == ServiceId
                        && m.CompletedSuccessful
                        select m;

                if (checkWithMasterId)
                {
                    q = from m in q
                        where m.Id < MasterLogId
                        select m;
                }

                q = from m in q
                    orderby m.Id descending
                    select m;

                //query executes here
                previousMaster = q.FirstOrDefault();
            }

            //      Logging.Log.Write(SeverityTypes.Verbose, "End of ThisFileHasntBeenProcessed() function in ServiceImporterBase");

            //if previous master doesn't exist, then there was no previous file
            if (previousMaster == null) return true;

            //This should never occur, but if it does, treat the version as a different file
            if (String.IsNullOrWhiteSpace(previousMaster.FileNames)) return true;

            //if the filenames are not the same, then the file is a new version
            return previousMaster.FileNames != FileNamesForDb;
        }

        private bool CheckFileExists()
        {
            return FilePaths.Select(FlatFiles.S3FileExists).All(exists => exists);
        }

        //reads the first line of each file in FilePaths, if an exception is thrown it
        //has most likely not transferred correctly yet.
        private bool CheckFileCanBeRead()
        {
            foreach (var filepath in FilePaths)
            {
                try
                {
                    using (var sr = new StreamReader(filepath))
                    {
                        if ((sr.ReadLine()) != null)
                        {
                            continue;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private void CreateMasterLog()
        {
            //      Logging.Log.Write(SeverityTypes.Verbose, "Start of CreateMasterLog() function in ServiceImporterBase");

            // KJ - There is an issue currently where the service seems to restart without properly cancelling the previous run. As it break the flow
            // if there are more than one incomplete run for any service, this code has been updated to continue with the previous ID rather than create a 
            // new one. 
            var timeFromStartOfToday = DateTime.Now - DateTime.Now.Date;
            var utcDateMinusTimeFromStartOfToday = DateTime.UtcNow - timeFromStartOfToday;

           

            var existing = Repository<ImportServiceMaster>
                        .Where(m => m.DateCreated > utcDateMinusTimeFromStartOfToday
                            && m.DateCreated < utcDateMinusTimeFromStartOfToday.AddDays(1)
                            && m.DateCompleted == null
                            && !m.MainProcessStarted
                            && m.ServiceId == ServiceId).ToList();
            if (existing.Any())
            {
                MasterLogId = existing.First().Id;
                Log(LogType.Log, "Existing Incomplete service found. Using this ID going forwards");
            }
           


            if (MasterLogId == 0)
            {
                // none existing create a new one
                var master = Repository.Insert(new ImportServiceMaster()
                {
                    ServiceId = ServiceId,
                    DateCreated = DateTime.UtcNow,
                    MainProcessStarted = false,
                    ProcessManuallyStopped = false
                });
                MasterLogId = master.Id;
            }

            SetStartTimeOfService();
            //    Logging.Log.Write(SeverityTypes.Verbose, "End of CreateMasterLog() function in ServiceImporterBase");
        }

        protected void SetStartTimeOfService()
        {
            var timeFromStartOfToday = DateTime.Now - DateTime.Now.Date;
            StartOfDayUTCWhenCreated = DateTime.UtcNow - timeFromStartOfToday;
        }

        protected void Log(LogType logType, string description)
        {

            Repository.Insert(new ImportServiceLog()
            {
                MasterServiceId = MasterLogId,
                LogType = (int)logType,
                Description = description,
                DateCreated = DateTime.UtcNow
            });

        }
    }
}
