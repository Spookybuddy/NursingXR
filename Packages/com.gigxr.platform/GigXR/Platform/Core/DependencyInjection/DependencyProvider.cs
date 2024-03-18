using System;
using System.Collections.Generic;

namespace GIGXR.Platform.Core.DependencyInjection
{
    public class DependencyProvider : IDependencyProvider
    {
        private readonly HashSet<Type> registeredDependencies = new HashSet<Type>();

        private readonly Dictionary<Type, Func<DependencyProvider, object>> transientFuncs =
            new Dictionary<Type, Func<DependencyProvider, object>>();

        private readonly Dictionary<Type, Lazy<object>> singletonLazies = new Dictionary<Type, Lazy<object>>();

        public DependencyProvider RegisterTransient<T>(Func<DependencyProvider, T> createInstanceAction) where T : class
        {
            var type = typeof(T);
            if (registeredDependencies.Contains(type))
            {
                return this;
            }

            transientFuncs[type] = createInstanceAction;
            registeredDependencies.Add(type);

            return this;
        }

        public DependencyProvider RegisterSingleton<T>(Func<DependencyProvider, T> createInstanceAction) where T : class
        {
            var type = typeof(T);
            if (registeredDependencies.Contains(type))
            {
                return this;
            }
            singletonLazies[type] = new Lazy<object>(() => createInstanceAction(this));
            registeredDependencies.Add(type);

            return this;
        }

        public DependencyProvider RegisterSingleton(Func<DependencyProvider, object> createInstanceAction, Type type)
        {
            if (registeredDependencies.Contains(type))
            {
                return this;
            }
            singletonLazies[type] = new Lazy<object>(() => createInstanceAction(this));
            registeredDependencies.Add(type);

            return this;
        }

        public T GetDependency<T>() where T : class
        {
            var type = typeof(T);
            if (transientFuncs.TryGetValue(type, out var transientCreateFunc))
            {
                return (T)transientCreateFunc(this);
            }

            if (singletonLazies.TryGetValue(type, out var singletonLazyInstance))
            {
                return (T)singletonLazyInstance.Value;
            }

            // This is an exceptional situation. Dependencies should always resolve to a value and
            // if they don't then something is set up incorrectly.
            throw new InvalidOperationException($"Cannot resolve dependency: {typeof(T)}");
        }

        public void DestroyAllDependencies()
        {
            registeredDependencies.Clear();

            foreach(var dependency in singletonLazies.Values)
            {
                if(dependency.IsValueCreated)
                {
                    if (dependency.Value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }

            singletonLazies.Clear();

            transientFuncs.Clear();
        }
    }
}