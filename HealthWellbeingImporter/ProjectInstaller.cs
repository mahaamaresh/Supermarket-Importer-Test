using System.ComponentModel;
using System.Configuration.Install;


namespace HealthWellbeingImporter
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
