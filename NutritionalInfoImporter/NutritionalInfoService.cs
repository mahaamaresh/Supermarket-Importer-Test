using System.ServiceProcess;
using System.Threading;

namespace NutritionalInfoImporter
{
    partial class NutritionalInfoService: ServiceBase
    {
        Importer importer;
        public NutritionalInfoService()
        {
            InitializeComponent();

            //if(!System.Diagnostics.EventLog.SourceExists("NutritionalInfoImporter"))
            //{
            //    System.Diagnostics.EventLog.CreateEventSource("NutritionalInfoImporter", "WoWImporterLog");
            //}

            //eventLog1.Source = "NutritionalInfoImporter";
            //eventLog1.Log = "WoWImporterLog";

            //eventLog1.WriteEntry("Init");

            importer = new Importer();
        }

        protected override void OnStart(string[] args)
        {
            //eventLog1.WriteEntry("In OnStart");
            
            (new Thread(new ThreadStart(importer.Start))).Start();            
        }

        protected override void OnStop()
        {
            //eventLog1.WriteEntry("In OnStop");
            importer.Stop();
        }
    }
}
