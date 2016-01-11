using System;
using System.Collections.Generic;
using ImporterBLL;
using ImporterBLL.Importers;
using ImporterBLL.Objects;
using RecipeImporter.Properties;

namespace RecipeImporter
{

    public class Importer : ErrorHandledService
    {
        private Recipe importer;

        /// <summary>
        /// Class constructor, sets the email and file log levels
        /// </summary>
        public Importer() {}

        protected override Constants.ProcessOutcome Process()
        {
            var fileNames = new List<string>();
            foreach (var fileName in Settings.Default.FileName.Split(','))
            {
                fileNames.Add(String.Format(fileName, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd")));
            }
            var featuredFileNames = new List<string>();
            foreach (var featured in Settings.Default.FeaturedFileName.Split(','))
            {
                featuredFileNames.Add(String.Format(featured, DateTime.Now.AddDays(Settings.Default.FeaturedFileNameDateCheckOffsetDays).ToString("yyyyMMdd")));
            }

            importer = new Recipe(Settings.Default.FilePath, Settings.Default.ArchivePath, Settings.Default.StagingTableName, Settings.Default.FormatFilePath,
                fileNames, featuredFileNames, Settings.Default.SummaryReportErrorToEmailAddress, Settings.Default.SummaryReportFromEmailAddress, Settings.Default.SummaryReportFromAddressFriendlyName, Settings.Default.ImageDirectory,
                Settings.Default.ImagePathURL, 
                Settings.Default.SqlaServerPath,
                Settings.Default.SqlbServerPath,
                Settings.Default.LocalSqlPath,
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
