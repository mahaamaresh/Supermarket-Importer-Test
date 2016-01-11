using System.ComponentModel;
using System.Configuration.Install;


namespace ProductLocationDailyImporter
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
