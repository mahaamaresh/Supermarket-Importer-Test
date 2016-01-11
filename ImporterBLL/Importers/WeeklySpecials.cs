using System;
using System.Collections.Generic;
using System.Linq;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class WeeklySpecials : ServiceImporterBase
    {
        private readonly string _fileName;

        public WeeklySpecials(string fileDirectoryPath, string archiveDirectoryPath, string stagingTableName, string formatFilePath, string fileName, string summaryReportErrorToEmailAddress,
            string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName,
            string sqlaServerPath, string sqlbServerPath, string localSqlPath, string tempUploadFolder,
            string daysToRun)
            : base(ServiceLogger.FlatFileType.WeeklySpecials, FlatFiles.FileFormat.TXT, FlatFiles.CompressType.Zip,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath,localSqlPath, tempUploadFolder, daysToRun)
        {
            _fileName = fileName;
        }

        protected override List<string> FileNames
        {
            get
            {
                return _fileName.Split(',').ToList();
            }
        }

        protected override bool ExecuteDataProcessingProc()
        {
            int success;

            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                success = db.p_ImportWeeklySpecial(MasterLogId);
            }

            if (success == 0) return false;
            else if (success == 1) return true;
            else throw new ArgumentOutOfRangeException("success", "Stored Proc p_ImportWeeklySpecial returned int value that that was not equal to 1 or 0");
        }

        protected override void ExecuteResetProc()
        {
            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                db.p_ImportWeeklySpecial_Reset();
            }
        }

        protected override string DataProcessProcName
        {
            get { return "p_ImportWeeklySpecial"; }
        }

        protected override string ResetProcName
        {
            get { return "p_ImportWeeklySpecial_Reset"; }
        }
    }
}
