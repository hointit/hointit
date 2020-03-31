using Prism.Ioc;
using Prism.Logging;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;

namespace PrismRole.Core
{
    public class RoleBasedModuleInitializer : IModuleInitializer
    {
        private readonly IContainerExtension _containerExtension;
        private readonly ILoggerFacade _loggerFacade;

        /// <summary>
        /// Initializes a new instance of <see cref="ModuleInitializer"/>.
        /// </summary>
        /// <param name="containerExtension">The container that will be used to resolve the modules by specifying its type.</param>
        /// <param name="loggerFacade">The logger to use.</param>
        public RoleBasedModuleInitializer(IContainerExtension containerExtension, ILoggerFacade loggerFacade)
        {
            this._containerExtension = containerExtension ?? throw new ArgumentNullException(nameof(containerExtension));
            this._loggerFacade = loggerFacade ?? throw new ArgumentNullException(nameof(loggerFacade));
        }

        /// <summary>
        /// Initializes the specified module.
        /// </summary>
        /// <param name="moduleInfo">The module to initialize</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catches Exception to handle any exception thrown during the initialization process with the HandleModuleInitializationError method.")]
        public void Initialize(IModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
                throw new ArgumentNullException(nameof(moduleInfo));

            IModule moduleInstance = null;
            try
            {
                if (ModuleIsInUserRole(moduleInfo))
                {
                    moduleInstance = this.CreateModule(moduleInfo);
                    if (moduleInstance != null)
                    {
                        moduleInstance.RegisterTypes(_containerExtension);
                        moduleInstance.OnInitialized(_containerExtension);
                    }
                }
            }
            catch (Exception ex)
            {
                this.HandleModuleInitializationError(
                    moduleInfo,
                    moduleInstance?.GetType().Assembly.FullName,
                    ex);
            }
        }

        private bool ModuleIsInUserRole(IModuleInfo moduleInfo)
        {
            bool isInRole = false;
            var roles = GetModuleRoles(moduleInfo);

            foreach (var role in roles)
            {
                if (Thread.CurrentPrincipal.IsInRole(role))
                {
                    isInRole = true;
                    break;
                }
            }
            return isInRole;
        }

        private IEnumerable<string> GetModuleRoles(IModuleInfo moduleInfo)
        {
            var type = Type.GetType(moduleInfo.ModuleType);
            foreach (var attr in GetCustomAttribute<RolesAttribute>(type))
            {
                return attr.Roles.AsEnumerable();
            }
            return null;
        }

        IEnumerable<T> GetCustomAttribute<T>(Type type)
        {
            var a = type.GetCustomAttributes(typeof(T), true).OfType<T>().ToList();
            return type.GetCustomAttributes(typeof(T), true).OfType<T>();
        }

        /// <summary>
        /// Handles any exception occurred in the module Initialization process,
        /// logs the error using the <see cref="ILoggerFacade"/> and throws a <see cref="ModuleInitializeException"/>.
        /// This method can be overridden to provide a different behavior.
        /// </summary>
        /// <param name="moduleInfo">The module metadata where the error happenened.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="exception">The exception thrown that is the cause of the current error.</param>
        /// <exception cref="ModuleInitializeException"></exception>
        public virtual void HandleModuleInitializationError(IModuleInfo moduleInfo, string assemblyName, Exception exception)
        {
            if (moduleInfo == null)
                throw new ArgumentNullException(nameof(moduleInfo));

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            Exception moduleException;

            if (exception is ModuleInitializeException)
            {
                moduleException = exception;
            }
            else
            {
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    moduleException = new ModuleInitializeException(moduleInfo.ModuleName, assemblyName, exception.Message, exception);
                }
                else
                {
                    moduleException = new ModuleInitializeException(moduleInfo.ModuleName, exception.Message, exception);
                }
            }

            //this._loggerFacade.Log(moduleException.ToString(), Category.Exception, Priority.High);

            throw moduleException;
        }

        /// <summary>
        /// Uses the container to resolve a new <see cref="IModule"/> by specifying its <see cref="Type"/>.
        /// </summary>
        /// <param name="moduleInfo">The module to create.</param>
        /// <returns>A new instance of the module specified by <paramref name="moduleInfo"/>.</returns>
        protected virtual IModule CreateModule(IModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
                throw new ArgumentNullException(nameof(moduleInfo));

            return this.CreateModule(moduleInfo.ModuleType);
        }

        /// <summary>
        /// Uses the container to resolve a new <see cref="IModule"/> by specifying its <see cref="Type"/>.
        /// </summary>
        /// <param name="typeName">The type name to resolve. This type must implement <see cref="IModule"/>.</param>
        /// <returns>A new instance of <paramref name="typeName"/>.</returns>
        protected virtual IModule CreateModule(string typeName)
        {
            Type moduleType = Type.GetType(typeName);
            return (IModule)_containerExtension.Resolve(moduleType);
        }
    }
}
