using System.ServiceProcess;
using System.Threading;

namespace BonusPointsImporter
{
    public partial class BonusPointsService : ServiceBase
    {
        Importer importer;
        public BonusPointsService()
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
