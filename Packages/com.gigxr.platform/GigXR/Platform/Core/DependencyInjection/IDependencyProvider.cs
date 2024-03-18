namespace GIGXR.Platform.Core.DependencyInjection
{
    public interface IDependencyProvider
    {
        T GetDependency<T>() where T : class;
    }
}