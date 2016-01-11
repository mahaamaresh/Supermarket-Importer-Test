using System;
using System.Collections.Generic;
using ImporterBLL;
using ImporterBLL.Importers;
using ImagesImporter.Properties;
using ImporterBLL.Objects;

namespace ImagesImporter
{
    public class Importer : ErrorHandledService
    {
        private Images importer;

        public Importer() {}

        protected override Constants.ProcessOutcome Process()
        {
            //Sigh. Slightly different Logic for PEL integration with this importer, PEL will have it's own settings for Images, meaning that both will have to be checked for adhoc builds.
            var fileNames = new List<string>();
            foreach (var digit in Settings.Default.FileList.Split(','))
            {
                fileNames.Add(String.Format(Settings.Default.FileName, digit, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyy-MM-dd")));
            }

            foreach (var digit in Settings.Default.PELFileList.Split(','))
            {
                fileNames.Add(String.Format(Settings.Default.PELFileName, digit, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDaysPEL).ToString("yyyyMMdd")));
            }

            importer = new Images(Settings.Default.FilePath, 
                Settings.Default.ArchivePath, fileNames,
                string.Empty, 
                string.Empty, 
                Settings.Default.SummaryReportErrorToEmailAddress, 
                Settings.Default.SummaryReportFromEmailAddress, 
                Settings.Default.SummaryReportFromAddressFriendlyName, 
                Settings.Default.DestinationPath,
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
