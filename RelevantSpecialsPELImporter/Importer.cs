using System;
using ImporterBLL.Objects;
using RelevantSpecialsPELImporter.Properties;
using ImporterBLL.Importers;
using ImporterBLL;

namespace RelevantSpecialsPELImporter
{
    public class Importer : ErrorHandledService
    {
        private RelevantSpecialsPEL _importer;

        protected override Constants.ProcessOutcome Process()
        {
            var fileName = String.Format(Settings.Default.FileName, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd"));

            _importer = new RelevantSpecialsPEL(Settings.Default.FilePath, Settings.Default.ArchivePath, Settings.Default.StagingTableName, Settings.Default.FormatFilePath,
                fileName, Settings.Default.SummaryReportErrorToEmailAddress, Settings.Default.SummaryReportFromEmailAddress, Settings.Default.SummaryReportFromAddressFriendlyName,
                Settings.Default.SqlaServerPath,
                Settings.Default.SqlbServerPath,
                Settings.Default.LocalSqlPath,
                Settings.Default.TemporaryUploadFolder,
                Settings.Default.DaysToRun);

            return _importer.Process();
        }

        public override void  Stop()
        {
            _importer.ManualStop();
            base.Cleanup();
        }
    }
}
