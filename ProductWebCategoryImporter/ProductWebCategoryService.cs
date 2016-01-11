using System.ServiceProcess;
using System.Threading;

namespace ProductWebCategoryImporter
{
    partial class ProductWebCategoryService : ServiceBase
    {
        Importer importer;
        public ProductWebCategoryService()
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
