﻿using System;
using ImporterBLL;
using ImporterBLL.Helpers;
using ImporterBLL.Importers;
using ImporterBLL.Objects;
using ProductMasterImporter.Properties;

namespace ProductMasterImporter
{
    public class Importer : ErrorHandledService
    {
        private ProductMaster importer;

        /// <summary>
        /// Class constructor, sets the email and file log levels
        /// </summary>
        public Importer() {}

        protected override Constants.ProcessOutcome Process()
        {
          

            var fileName = String.Format(Settings.Default.FileName, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd"));

            importer = new ProductMaster(Settings.Default.FilePath, Settings.Default.ArchivePath, Settings.Default.StagingTableName, Settings.Default.FormatFilePath,
                fileName, Settings.Default.SummaryReportErrorToEmailAddress, Settings.Default.SummaryReportFromEmailAddress, Settings.Default.SummaryReportFromAddressFriendlyName, Settings.Default.ImageURL, Settings.Default.PELImageUrl,
                Settings.Default.SqlaServerPath, Settings.Default.SqlbServerPath, Settings.Default.LocalSqlPath,
                Settings.Default.TemporaryUploadFolder,
                Settings.Default.DaysToRun);

            return importer.Process();
        }

        public override void Stop()
        {
            importer.ManualStop();
            base.Stop();
        }
    }
}
