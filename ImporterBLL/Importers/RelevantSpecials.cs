using System;
using System.Collections.Generic;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class RelevantSpecials : ServiceImporterBase
    {
        private readonly string _fileName;

        public RelevantSpecials(string fileDirectoryPath, string archiveDirectoryPath, string stagingTableName, string formatFilePath, string fileName, string summaryReportErrorToEmailAddress,
            string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, string sqlaServerPath, string sqlbServerPath, string localSqlPath,  string tempUploadFolder,
            string daysToRun)
            : base(ServiceLogger.FlatFileType.RelevantSpecials, FlatFiles.FileFormat.CSV, FlatFiles.CompressType.GZip,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath, localSqlPath, tempUploadFolder, daysToRun: daysToRun)
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
                success = db.p_ImportRelevantSpecial(MasterLogId); 
            }

            switch (success)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException("success", "Stored Proc p_ImportRelevantSpecial returned int value that that was not equal to 1 or 0");
            }
        }

        protected override void ExecuteResetProc()
        {
            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                db.p_ImportRelevantSpecial_Reset();
            }
        }

        protected override string DataProcessProcName
        {
            get { return "p_ImportRelevantSpecial"; }
        }

        protected override string ResetProcName
        {
            get { return "p_ImportRelevantSpecial_Reset"; }
        }
    }
}
