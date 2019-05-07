using System.ComponentModel;

namespace IPTVTuner
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void ServiceProcessInstaller1_AfterInstall(object sender, System.Configuration.Install.InstallEventArgs e)
        {

        }
    }
}
