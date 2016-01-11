using System.ServiceProcess;
using System.Threading;

namespace ProductLocationDailyImporter
{
    partial class ProductLocationDailyService : ServiceBase
    {
        Importer importer;
        public ProductLocationDailyService()
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
