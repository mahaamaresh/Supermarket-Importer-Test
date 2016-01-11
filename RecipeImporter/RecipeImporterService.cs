using System.ServiceProcess;
using System.Threading;

namespace RecipeImporter
{
    partial class RecipeImporterService : ServiceBase
    {
        Importer importer;
        public RecipeImporterService()
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
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            importer.Stop();
        }
    }
}
