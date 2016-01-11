using System.ServiceProcess;
using System.Threading;

namespace ImagesImporter
{
    partial class ImagesImporterService : ServiceBase
    {

        Importer importer;
        public ImagesImporterService()
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
