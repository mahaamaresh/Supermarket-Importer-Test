using System;
using System.Collections.Generic;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class ProductPrice : ServiceImporterBase
    {
        private readonly string _fileName;

        public ProductPrice(string fileDirectoryPath, string fileName, string stagingTableName, string formatFilePath, string archiveDirectoryPath, string summaryReportErrorToEmailAddress,
            string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, string sqlaServerPath, string sqlbServerPath, string localSqlPath, string tempUploadFolder,
            string daysToRun)
            : base(ServiceLogger.FlatFileType.ProductPriceRanging, FlatFiles.FileFormat.TXT, FlatFiles.CompressType.Zip,
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

            using (var context = new WoolworthsDBDataContext())
            {
                context.CommandTimeout = CommandTimeoutInSeconds.Value;
                success = context.p_Import_PriceData_ProductRangePricing(MasterLogId);
            }

            if (success == 0) return false;
            else if (success == 1) return true;
            else throw new ArgumentOutOfRangeException("success", "Stored Proc p_Import_PriceData_ProductRangePricing returned int value that that was not equal to 1 or 0");
        }

        protected override void ExecuteResetProc()
        {
            using (var context = new WoolworthsDBDataContext())
            {
                context.CommandTimeout = CommandTimeoutInSeconds.Value;
                context.p_Import_PriceData_ProductRangePricing_Reset();
            }
        }

        protected override string DataProcessProcName
        {
            get { return "p_Import_PriceData_ProductRangePricing"; }
        }

        protected override string ResetProcName
        {
            get { return "p_Import_PriceData_ProductRangePricing_Reset"; }
        }
    }
}
