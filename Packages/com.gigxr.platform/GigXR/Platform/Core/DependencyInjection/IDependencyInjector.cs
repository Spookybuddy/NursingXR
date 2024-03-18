using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GIGXR.Platform.Core.DependencyInjection
{
    public interface IDependencyInjector
    {
        UniTask InjectIntoMonoBehaviours();

        UniTask InjectIntoMonoBehaviours(IEnumerable<MonoBehaviour> monoBehaviours);

        UniTask InjectIntoMonoBehaviours(MonoBehaviour monoBehaviour);

        UniTask InjectIntoMonoBehavioursMethods(IUniTaskAsyncEnumerable<(MonoBehaviour, MethodInfo)> monoBehaviours);
    }
}