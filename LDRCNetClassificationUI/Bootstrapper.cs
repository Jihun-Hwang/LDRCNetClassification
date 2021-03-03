using Prism.Unity;
using System.Windows;

namespace LDRCNetClassificationUI
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override void InitializeShell()
        {
            Application.Current.MainWindow = Container.TryResolve<MainWindow>();
            Application.Current.MainWindow.Show();
        }
    }
}
