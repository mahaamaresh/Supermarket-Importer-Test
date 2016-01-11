using System.ServiceProcess;
using System.Threading;

namespace AssortmentImporter
{
    public partial class AssortmentService : ServiceBase
    {
        readonly Importer importer;
        public AssortmentService()
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
