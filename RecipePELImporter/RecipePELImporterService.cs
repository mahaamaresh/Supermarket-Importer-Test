using System.ServiceProcess;
using System.Threading;

namespace RecipePELImporter
{
    partial class RecipePELImporterService : ServiceBase
    {
        Importer importer;
        public RecipePELImporterService()
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
