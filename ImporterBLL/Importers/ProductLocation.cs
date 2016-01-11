using System;
using System.Collections.Generic;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class ProductLocation : ServiceImporterBase
    {

        private List<string> _fileNames = new List<string>();

        public ProductLocation(string fileDirectoryPath, string archiveDirectoryPath, List<string> fileNames, string stagingTableName, string formatFilePath,
            string summaryReportErrorToEmailAddress, string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, string sqlaServerPath, string sqlbServerPath, string localSqlPath, string tempUploadFolder,
            string daysToRun)
            : base(ServiceLogger.FlatFileType.ProductLocation, FlatFiles.FileFormat.TXT, FlatFiles.CompressType.Zip,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath, localSqlPath, tempUploadFolder, daysToRun: daysToRun)
        {
            foreach (var fileName in fileNames)
            {
                _fileNames.Add(fileName);
            }
        }

        protected override bool ExecuteDataProcessingProc()
        {
            int success;

            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                success = db.p_Import_MasterProductLoc_ProductLocation(MasterLogId); 
            }

            if (success == 0) return false;
            else if (success == 1) return true;
            else throw new ArgumentOutOfRangeException("success", "Stored Proc p_Import_MasterProductLoc_ProductLocation returned int value that that was not equal to 1 or 0");
        }

        protected override void ExecuteResetProc()
        {
            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                db.p_Import_MasterProductLoc_ProductLocation_Reset();
            }
        }

        protected override List<string> FileNames
        {
            get { return _fileNames; }
        }

        protected override string DataProcessProcName
        {
            get { return "p_Import_MasterProductLoc_ProductLocation"; }
        }

        protected override string ResetProcName
        {
            get { return "p_Import_MasterProductLoc_ProductLocation_Reset"; }
        }
    }
}
