using System.ComponentModel;
using System.Configuration.Install;


namespace ProductWebCategoryImporter
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
