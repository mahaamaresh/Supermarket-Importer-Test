using System;
using ImporterBLL.Objects;
using NutritionalInfoImporter.Properties;
using ImporterBLL.Importers;
using ImporterBLL;


namespace NutritionalInfoImporter
{
    public class Importer: ErrorHandledService
    {
        private NutritionalInfo importer;

        /// <summary>
        /// sets the email and file log levels
        /// </summary>
        public Importer()
        {
            //this.eventLog1 = new System.Diagnostics.EventLog();
            //((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            //((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            //eventLog1.Source = "SourceNutritionalInfoImporter";
            //eventLog1.Log = "InfoLog";
        }

        protected override Constants.ProcessOutcome Process()
        {
            //eventLog1.WriteEntry("In Importer Process");
            var fileName = String.Format(
                Settings.Default.FileName,
                DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd"));

            importer = new NutritionalInfo(
                Settings.Default.FilePath,
                fileName,
                Settings.Default.StagingTableName,
                Settings.Default.FormatFilePath,
                Settings.Default.ArchivePath,
                Settings.Default.SummaryReportErrorToEmailAddress,
                Settings.Default.SummaryReportFromEmailAddress,
                Settings.Default.SummaryReportFromAddressFriendlyName,
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
