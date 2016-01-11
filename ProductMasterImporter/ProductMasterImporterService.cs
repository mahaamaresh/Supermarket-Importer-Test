using System.ServiceProcess;
using System.Threading;

namespace ProductMasterImporter
{
    partial class ProductMasterImporterService : ServiceBase
    {
        Importer importer;
        public ProductMasterImporterService()
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
