using System;
using ImporterBLL;
using HealthWellbeingImporter.Properties;
using ImporterBLL.Importers;
using ImporterBLL.Objects;

namespace HealthWellbeingImporter
{
    public class Importer : ErrorHandledService
    {
        private HealthWellbeing importer;

        /// <summary>
        /// sets the email and file log levels
        /// </summary>
        public Importer() {}

        protected override Constants.ProcessOutcome Process()
        {
            var fileName = String.Format(Settings.Default.FileName, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd"));

            importer = new HealthWellbeing(Settings.Default.FilePath, fileName, Settings.Default.StagingTableName,
            Settings.Default.FormatFilePath, Settings.Default.ArchivePath, Settings.Default.SummaryReportErrorToEmailAddress, Settings.Default.SummaryReportFromEmailAddress, Settings.Default.SummaryReportFromAddressFriendlyName,
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
