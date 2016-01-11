using System.ServiceProcess;
using System.Threading;


namespace OnlineSellTypeImporter
{
    public partial class OnlineSellTypeService : ServiceBase
    {
        Importer importer;
        public OnlineSellTypeService()
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
