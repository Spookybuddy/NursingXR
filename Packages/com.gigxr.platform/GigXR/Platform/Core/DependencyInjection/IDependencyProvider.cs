using System;

namespace GIGXR.Platform.Core.DependencyInjection
{
    public interface IDependencyProvider
    {
        T GetDependency<T>() where T : class;

        DependencyProvider RegisterSingleton<T>(Func<DependencyProvider, T> createInstanceAction) where T : class;
    }
}