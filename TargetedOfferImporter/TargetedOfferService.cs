using System.ServiceProcess;
using System.Threading;

namespace TargetedOfferImporter
{
    public partial class TargetedOfferService : ServiceBase
    {
        Importer importer;
        public TargetedOfferService()
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
