using System;
using BonusPointsImporter.Properties;
using ImporterBLL.Importers;
using ImporterBLL;
using ImporterBLL.Objects;

namespace BonusPointsImporter
{
    public class Importer : ErrorHandledService
    {
        private BonusPoints importer;

        /// <summary>
        /// sets the email and file log levels
        /// </summary>
        public Importer() {}

        protected override Constants.ProcessOutcome Process()
        {
            var fileName = String.Format(Settings.Default.FileName, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd"));

            importer = new BonusPoints(Settings.Default.FilePath, fileName, Settings.Default.StagingTableName,
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
