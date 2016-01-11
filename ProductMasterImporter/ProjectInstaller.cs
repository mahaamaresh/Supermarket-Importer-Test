using System.ComponentModel;
using System.Configuration.Install;


namespace ProductMasterImporter
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
