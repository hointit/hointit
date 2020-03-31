using Unity;
using Prism.Unity;
using Prism.Modularity;
using System.Windows;
using Unity.Lifetime;
using PrismRole.Views;
using ModuleA;
using ModuleU;
using System.Security.Principal;
using System.Threading;

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
            var ident = WindowsIdentity.GetCurrent();
            var p = new GenericPrincipal(ident, new string[] {"Admin" });
            Thread.CurrentPrincipal = p;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            ModuleCatalog.AddModule<ModuleAModule>();
            ModuleCatalog.AddModule<ModuleUModule>();
            Container.RegisterType<IModuleInitializer, Core.RoleBasedModuleInitializer>(new ContainerControlledLifetimeManager());
        }
    }
}
