using System;
using System.Collections.Generic;
using ImporterBLL;
using ImporterBLL.Importers;
using ImporterBLL.Objects;
using ProductLocationDailyImporter.Properties;

namespace ProductLocationDailyImporter
{
    public class Importer : ErrorHandledService
    {
        private ProductLocationDaily importer;

        public Importer()
        {
        }

        protected override Constants.ProcessOutcome Process()
        {
            var fileNames = new List<string>();
            foreach (var state in Settings.Default.StateList.Split(','))
            {
                fileNames.Add(String.Format(Settings.Default.FileName, state, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd")));
            }

            importer = new ProductLocationDaily(Settings.Default.FilePath, Settings.Default.ArchivePath, Settings.Default.StagingTableName, Settings.Default.FormatFilePath,
                fileNames, Settings.Default.SummaryReportErrorToEmailAddress, Settings.Default.SummaryReportFromEmailAddress, Settings.Default.SummaryReportFromAddressFriendlyName,
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
