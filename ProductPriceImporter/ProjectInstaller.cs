using System.ComponentModel;
using System.Configuration.Install;


namespace ProductPriceImporter
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void ProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
