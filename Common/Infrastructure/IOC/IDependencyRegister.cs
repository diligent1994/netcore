using Autofac;


namespace Common.Infrastructure.IOC
{
    public interface IDependencyRegister
    {
        void Register(ContainerBuilder builder);
    }
}
