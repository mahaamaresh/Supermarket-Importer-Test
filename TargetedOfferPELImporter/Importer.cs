using System;
using ImporterBLL.Importers;
using ImporterBLL.Objects;
using TargetedOfferPELImporter.Properties;
using ImporterBLL;

namespace TargetedOfferPELImporter
{
    public class Importer : ErrorHandledService
    {
        private TargetedOfferPEL importer;
        public Importer() {}

        protected override Constants.ProcessOutcome Process()
        {
            var fileName = String.Format(Settings.Default.FileName, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd"));

            importer = new TargetedOfferPEL(Settings.Default.FilePath, Settings.Default.ArchivePath, Settings.Default.StagingTableName, Settings.Default.FormatFilePath,
                        fileName, Settings.Default.SummaryReportErrorToEmailAddress, Settings.Default.SummaryReportFromEmailAddress, Settings.Default.SummaryReportFromAddressFriendlyName, Settings.Default.ApnMessage, Settings.Default.SendApns,
                        Settings.Default.RecordApnStats,
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
