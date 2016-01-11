using System;
using System.Collections.Generic;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class MultibuyOffer : ServiceImporterBase
    {
        private readonly string _fileName;

        public MultibuyOffer(string fileDirectoryPath, string archiveDirectoryPath, string stagingTableName, string formatFilePath, string fileName, string summaryReportErrorToEmailAddress,
            string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, string sqlaServerPath, string sqlbServerPath, string localSqlPath, string tempUploadFolder,
            string daysToRun)
            : base(ServiceLogger.FlatFileType.MultibuyOffers, FlatFiles.FileFormat.TXT, FlatFiles.CompressType.Zip,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath,localSqlPath, tempUploadFolder, daysToRun)
        {
            _fileName = fileName;
        }

        protected override List<string> FileNames
        {
            get
            {
                return new List<string>() {
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
                success = db.p_ImportOffer(MasterLogId); //for now
            }

            switch (success)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException("success", "Stored Proc p_ImportOffer returned int value that that was not equal to 1 or 0");
            }
        }

        protected override void ExecuteResetProc()
        {
            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                db.p_ImportOffer_Reset(); //for now
            }
        }

        protected override string DataProcessProcName
        {
            get { return "p_ImportOffer"; }
        }

        protected override string ResetProcName
        {
            get { return "p_ImportOffer_Reset"; }
        }
    }
}
