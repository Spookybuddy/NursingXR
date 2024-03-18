using UnityEngine;

namespace GIGXR.Platform.Core.DependencyValidator
{
    /// <summary>
    /// An attribute to mark a dependency that is intended to be wired up in the inspector. Using this attribute allows
    /// design-time checks for dependencies to reduce runtime NullReferenceExceptions.
    ///
    /// Usage:
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [SerializeField, RequireDependency]
    ///     private object someDependency;
    /// }
    /// </code>
    /// </summary>
    public class RequireDependency : PropertyAttribute
    {
    }
}