using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GIGXR.Platform.Core.DependencyInjection
{
    // TODO: Add a validator for this as well to check that all methods that are expecting injections have
    // something registered.
    // TODO: Probably combine with DependencyValidator into the same namespace.
    public class DependencyInjector : IDependencyInjector
    {
        private readonly IDependencyProvider dependencyProvider;

        public DependencyInjector(IDependencyProvider dependencyProvider)
        {
            this.dependencyProvider = dependencyProvider;
        }

        public UniTask InjectIntoMonoBehaviours(IEnumerable<MonoBehaviour> monoBehaviours)
        {
            foreach (var monoBehaviour in monoBehaviours)
            {
                InjectIntoMonoBehaviour(monoBehaviour);
            }

            return UniTask.CompletedTask;
        }

        public UniTask InjectIntoMonoBehaviours(MonoBehaviour monoBehaviour)
        {
            InjectIntoMonoBehaviour(monoBehaviour);

            return UniTask.CompletedTask;
        }

        public async UniTask InjectIntoMonoBehaviours()
        {
            var monoBehaviours = FindMonoBehaviours();

            await InjectIntoMonoBehaviours(monoBehaviours);
        }

        public async UniTask InjectIntoMonoBehavioursMethods(IUniTaskAsyncEnumerable<(MonoBehaviour, MethodInfo)> methods)
        {
            await methods.ForEachAsync(method => InjectIntoMethod(method.Item1, method.Item2));
        }

        internal IEnumerable<MonoBehaviour> FindMonoBehaviours()
        {
            return Object.FindObjectsOfType<MonoBehaviour>(true);
        }

        internal void InjectIntoMonoBehaviour(MonoBehaviour monoBehaviour)
        {            
            foreach(var method in GetMethodsWithInjectAttribute(monoBehaviour.GetType()))
            {
                InjectIntoMethod(monoBehaviour, method);
            }
        }

        internal IEnumerable<MethodInfo> GetMethodsWithInjectAttribute(Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                       .Where(methodInfo => methodInfo.GetCustomAttributes<InjectDependencies>(true).Any());
        }

        internal void InjectIntoMethod(MonoBehaviour monoBehaviour, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            object[] parameterObjectsToInject = new object[parameterInfos.Length];

            for(int n = 0; n < parameterInfos.Length; n++)
            {
                var parameterType = parameterInfos[n].ParameterType;
                var dependency = RuntimeGetDependency(parameterType);
                var injectionTargetName = $"{monoBehaviour.GetType().Name}.{methodInfo.Name}";

                if (dependency == null)
                {
                    Debug.LogError($"DependencyInjector: Could not resolve instance of {parameterType} for injection into {injectionTargetName}!");
                    return;
                }

                parameterObjectsToInject[n] = dependency;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                // Debug.Log($"DependencyInjector: Injecting {((object)dependency).GetType().Name} into {injectionTargetName}");
#endif
            }

            methodInfo.Invoke(monoBehaviour, parameterObjectsToInject);
        }

        [CanBeNull]
        internal object RuntimeGetDependency(Type type)
        {
            var method = dependencyProvider
                .GetType()
                .GetMethod(nameof(IDependencyProvider.GetDependency), BindingFlags.Instance | BindingFlags.Public);
            var genericMethod = method?.MakeGenericMethod(type);
            return genericMethod?.Invoke(dependencyProvider, null);
        }
    }
}