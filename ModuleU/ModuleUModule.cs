using ModuleU.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using PrismRole.Core;

namespace ModuleU
{
    [Roles("User", "Admin")]
    public class ModuleUModule:IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("UserRegion", typeof(ViewU));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
