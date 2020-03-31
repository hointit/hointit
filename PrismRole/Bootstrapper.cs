using Unity;
using Prism.Unity;
using Prism.Modularity;
using System.Windows;
using Unity.Lifetime;
using PrismRole.Views;
using ModuleA;

namespace PrismRole
{
    public class Bootstrapper : UnityBootstrapper
    {

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            ModuleCatalog.AddModule<ModuleAModule>();

            //Container.RegisterType<IModuleInitializer, Core.RoleBasedModuleInitializer>(
            //    new ContainerControlledLifetimeManager());

        }
    }
}
