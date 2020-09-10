using Autofac;
using System;
using System.Linq;
using System.Reflection;

namespace Common.Infrastructure.IOC
{
    public class DependencyRegister : IDependencyRegister
    {
        public void Register(ContainerBuilder builder)
        {
            // auto register type.
            builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                              .Where(t => t.GetCustomAttribute<InjectableAttribute>() != null)
                              .AsImplementedInterfaces()
                              .InstancePerLifetimeScope();
        }
    }
}
