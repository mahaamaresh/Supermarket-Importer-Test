using System;
using System.Collections.Generic;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class RelevantSpecialsPEL : ServiceImporterBase
    {
        private readonly string _fileName;

        public RelevantSpecialsPEL(string fileDirectoryPath, string archiveDirectoryPath, string stagingTableName, string formatFilePath, string fileName, string summaryReportErrorToEmailAddress,
            string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, string sqlaServerPath, string sqlbServerPath, string localSqlPath, string tempUploadFolder,
            string daysToRun)
            : base(ServiceLogger.FlatFileType.RelevantSpecialsPEL, FlatFiles.FileFormat.CSV, FlatFiles.CompressType.Zip,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath,localSqlPath, tempUploadFolder, daysToRun: daysToRun)
        {
            _fileName = fileName; 
        }

        protected override List<string> FileNames
        {
            get
            {
                return new List<string>()
                {
                    { _fileName }
                };
            }
        }

        protected override bool ExecuteDataProcessingProc()
        {
            int success;

            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                success = db.p_ImportRelevantSpecialPEL(MasterLogId); 
            }

            if (success == 0) return false;
            else if (success == 1) return true;
            else throw new ArgumentOutOfRangeException("success", "Stored Proc p_ImportRelevantSpecialPEL returned int value that that was not equal to 1 or 0");
        }

        protected override void ExecuteResetProc()
        {
            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                db.p_ImportRelevantSpecialPEL_Reset();
            }
        }

        protected override string DataProcessProcName
        {
            get { return "p_ImportRelevantSpecialPEL"; }
        }

        protected override string ResetProcName
        {
            get { return "p_ImportRelevantSpecialPEL_Reset"; }
        }
    }
}
