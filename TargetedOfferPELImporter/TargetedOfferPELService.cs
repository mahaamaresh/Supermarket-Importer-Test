using System.ServiceProcess;
using System.Threading;

namespace TargetedOfferPELImporter
{
    public partial class TargetedOfferPELService : ServiceBase
    {
        Importer importer;
        public TargetedOfferPELService()
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
