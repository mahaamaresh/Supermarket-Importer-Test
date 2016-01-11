using System;
using System.Linq;
using ImporterBLL;
using ImporterBLL.Importers;
using ImporterBLL.Objects;
using WeeklySpecialsImporter.Properties;
using System.IO;

namespace WeeklySpecialsImporter
{
    public class Importer : ErrorHandledService
    {
        private WeeklySpecials importer;

        public Importer() {}
                
        protected override Constants.ProcessOutcome Process()
        {
            var fileName = String.Format(Settings.Default.FileName, DateTime.Now.AddDays(Settings.Default.FileNameDateCheckOffsetDays).ToString("yyyyMMdd"));
            //fileName = "SPL_PRMO_DATA_NEWFORMAT_FIX.txt";
            var formatFilePath = Settings.Default.FormatFilePath;

            //todo: take it out after the shelftalkertype file format transition is done
            if(File.Exists(Settings.Default.FilePath + fileName))
            {
                using(var reader = new StreamReader(Settings.Default.FilePath + fileName))
                {
                    var line = reader.ReadLine();

                    // new file format
                    if(line.Count() == 421) formatFilePath = formatFilePath.Replace(".txt", "-shelftalker.txt");

                    //Environment.Exit(0);
                }
            }

            importer = new WeeklySpecials(
                Settings.Default.FilePath, 
                Settings.Default.ArchivePath, 
                Settings.Default.StagingTableName, 
                formatFilePath,
                fileName, 
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
