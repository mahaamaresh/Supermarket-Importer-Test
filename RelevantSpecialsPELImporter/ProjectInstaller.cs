﻿using System.ComponentModel;
using System.Configuration.Install;


namespace RelevantSpecialsPELImporter
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
