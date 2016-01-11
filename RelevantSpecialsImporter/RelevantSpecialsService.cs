using System.ServiceProcess;
using System.Threading;

namespace RelevantSpecialsImporter
{
    public partial class RelevantSpecialsService : ServiceBase
    {
        Importer importer;
        public RelevantSpecialsService()
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
