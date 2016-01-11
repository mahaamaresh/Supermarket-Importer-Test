using System.ServiceProcess;
using System.Threading;

namespace RelevantSpecialsPELImporter
{
    partial class RelevantSpecialsPELService : ServiceBase
    {

        Importer importer;
        public RelevantSpecialsPELService()
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
