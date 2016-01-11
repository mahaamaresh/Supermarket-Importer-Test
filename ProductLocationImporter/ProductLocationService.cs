using System.ServiceProcess;
using System.Threading;

namespace ProductLocationImporter
{
    public partial class ProductLocationService : ServiceBase
    {
        Importer importer;
        public ProductLocationService()
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
