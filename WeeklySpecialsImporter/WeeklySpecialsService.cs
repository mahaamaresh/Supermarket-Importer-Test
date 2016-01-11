using System.ServiceProcess;
using System.Threading;


namespace WeeklySpecialsImporter
{
    public partial class WeeklySpecialsService : ServiceBase
    {
        Importer importer;
        public WeeklySpecialsService()
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
