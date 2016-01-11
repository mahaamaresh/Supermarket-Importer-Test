using System;
using ImporterBLL;
using ImporterBLL.Importers;
using ImporterBLL.Objects;
using MultibuyOfferImporter.Properties;

namespace MultibuyOfferImporter
{
    public class Importer : ErrorHandledService
    {
        private MultibuyOffer importer;

        public Importer() {}
        protected override Constants.ProcessOutcome Process()
        {
            var fileName = String.Format(Settings.Default.FileName, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd"));

            importer = new MultibuyOffer(Settings.Default.FilePath, Settings.Default.ArchivePath, Settings.Default.StagingTableName, Settings.Default.FormatFilePath,
                fileName, Settings.Default.SummaryReportErrorToEmailAddress, Settings.Default.SummaryReportFromEmailAddress, Settings.Default.SummaryReportFromAddressFriendlyName,
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
