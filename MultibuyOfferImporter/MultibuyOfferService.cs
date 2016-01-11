using System.ServiceProcess;
using System.Threading;

namespace MultibuyOfferImporter
{
    public partial class MultibuyOfferService : ServiceBase
    {
        Importer importer;
        public MultibuyOfferService()
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
