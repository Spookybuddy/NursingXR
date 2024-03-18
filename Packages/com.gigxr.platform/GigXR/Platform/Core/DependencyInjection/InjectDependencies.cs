using System;
using JetBrains.Annotations;

namespace GIGXR.Platform.Core.DependencyInjection
{
    /// <summary>
    /// Used to inject C# classes into MonoBehaviours.
    ///
    /// Usage:
    /// <code>
    /// public SomeBehaviour : MonoBehaviour
    /// {
    ///     private ISomeDependency SomeDependency { get; set; }
    /// 
    ///     [InjectDependencies]
    ///     public void Construct(ISomeDependency someDependency)
    ///     {
    ///         SomeDependency = someDependency;
    ///     }
    /// }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method), UsedImplicitly]
    public class InjectDependencies : Attribute
    {
    }
}