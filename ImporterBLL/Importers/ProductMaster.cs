﻿using System;
using System.Collections.Generic;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class ProductMaster : ServiceImporterBase
    {
        private readonly string _fileName;
        private readonly string _imageUrlFormat;
        private readonly string _pelImageUrlFormat;

        public ProductMaster(string fileDirectoryPath, string archiveDirectoryPath, string stagingTableName, string formatFilePath, string fileName,
            string summaryReportErrorToEmailAddress, string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, string imageUrlFormat, string pelImageUrlFormat,
            string sqlaServerPath, string sqlbServerPath, string localSqlPath, string tempUploadFolder,
            string daysToRun)
            : base(ServiceLogger.FlatFileType.ProductMaster, FlatFiles.FileFormat.TXT, FlatFiles.CompressType.Zip,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath,localSqlPath, tempUploadFolder, daysToRun)
        {
            _fileName = fileName;
            _imageUrlFormat = imageUrlFormat;
            _pelImageUrlFormat = pelImageUrlFormat;


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
                success = db.p_ImportProduct(MasterLogId, _imageUrlFormat, _pelImageUrlFormat); 
            }

            if (success == 0) return false;
            else if (success == 1) return true;
            else throw new ArgumentOutOfRangeException("success", "Stored Proc p_ImportProduct returned int value that that was not equal to 1 or 0");
        }

        protected override void ExecuteResetProc()
        {
            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                db.p_ImportProduct_Reset();
            }
        }

        protected override string DataProcessProcName
        {
            get { return "p_ImportProduct"; }
        }

        protected override string ResetProcName
        {
            get { return "p_ImportProduct_Reset"; }
        }
    }
}
