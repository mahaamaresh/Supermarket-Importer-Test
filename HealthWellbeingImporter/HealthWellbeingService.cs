using System.ServiceProcess;
using System.Threading;

namespace HealthWellbeingImporter
{
    partial class HealthWellbeingService : ServiceBase
    {
        Importer importer;
        public HealthWellbeingService()
        {
            InitializeComponent();
            importer = new Importer();
        }

        protected override void OnStart(string[] args)
        {
            (new Thread(new ThreadStart(importer.Start))).Start();
        }

        protected override void OnStop()
        {
            importer.Stop();
        }
    }
}
