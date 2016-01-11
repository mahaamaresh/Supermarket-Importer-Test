using System.ServiceProcess;
using System.Threading;

namespace ProductPriceImporter
{
    public partial class ProductPriceService : ServiceBase
    {
        Importer importer;
        public ProductPriceService()
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
