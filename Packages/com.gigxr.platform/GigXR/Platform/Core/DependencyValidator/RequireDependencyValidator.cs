using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GIGXR.Platform.Core.DependencyValidator
{
    using Microsoft.MixedReality.Toolkit;
    using Scenarios.GigAssets;
    using System;
    using System.Collections.Generic;
    using Object = Object;

    public class RequireDependencyValidator
    {
#if UNITY_EDITOR
        /// <summary>
        /// Adds a menu item in the editor to check required dependencies.
        /// </summary>
        [MenuItem("GIGXR/Health Checks/Validate Required Dependencies")]
        private static void ValidateRequiredDependenciesMenuOption()
        {
            var validator = new RequireDependencyValidator();

            if (validator.ValidateRequiredDependenciesInScene())
                Debug.Log("All [RequireDependency] fields have references!");
        }
#endif

#if DEVELOPMENT_BUILD
        /// <summary>
        /// Checks required dependencies at runtime when ran as a development build.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void ValidateRequiredDependenciesOnStartup()
        {
            var validator = new RequireDependencyValidator();
            validator.ValidateRequiredDependenciesInScene();
        }
#endif

        /// <summary>
        /// Checks MonoBehaviours on loaded GameObjects for fields with the [RequireDependency] attribute and verifies
        /// if they have values set.
        /// </summary>
        /// <returns>Whether all fields with the [RequireDependency] attribute are non-null.</returns>
        public bool ValidateRequiredDependenciesInScene()
        {
            var dependenciesValid = true;

            // Find all MonoBehaviours.
            var monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();

            foreach (var monoBehaviour in monoBehaviours)
            {
                if (!ValidateMonoBehaviour(monoBehaviour))
                {
                    dependenciesValid = false;
                }
            }

            return dependenciesValid;
        }

        /// <summary>
        /// Checks MonoBehaviours on provided GameObjects for fields with the [RequireDependency] attribute and verifies
        /// if they have values set.
        /// </summary>
        /// <returns>Whether all fields with the [RequireDependency] attribute are non-null.</returns>
        public bool ValidateRequiredDependencies(GameObject[] gameObjects)
        {
            var dependenciesValid = true;

            // Find all MonoBehaviours.
            foreach (var gameObject in gameObjects)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                // Debug.Log($"Checking dependencies on GameObject: {gameObject.name}");
#endif

                var behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();
                if (behaviours == null) continue;

                foreach (var behaviour in behaviours)
                {
                    if (!ValidateMonoBehaviour(behaviour))
                    {
                        dependenciesValid = false;
                    }
                }
            }

            return dependenciesValid;
        }

        /// <summary>
        /// Checks provided MonoBehaviours for fields with the [RequireDependency] attribute and verifies
        /// if they have values set.
        /// </summary>
        /// <returns>Whether all fields with the [RequireDependency] attribute are non-null.</returns>
        private bool ValidateMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            var dependenciesValid = true;
            var monoBehaviourType = monoBehaviour.GetType();

            // Find private fields with the [RequireDependency] attribute.
            var fieldInfos = GetRequireDependencyFieldInfos(monoBehaviourType);
            if (fieldInfos.Count == 0) return true;

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldValue = fieldInfo.GetValue(monoBehaviour);

                if (fieldValue == null ||
                    fieldValue.Equals(default))
                {
                    dependenciesValid = false;

                    Debug.LogError
                        (
                            $"Required dependency missing: {monoBehaviourType.Name}.{fieldInfo.Name}. You need to wire it up in the inspector!",
                            monoBehaviour.gameObject
                        );

                    // Tell the user which AT this is on, if applicable. 
                    var mediator = monoBehaviour.gameObject.FindAncestorComponent<AssetMediator>();
                    if (mediator)
                        Debug.LogError($"Required dependency missing in AT: {mediator.name}");
                }
            }

            return dependenciesValid;
        }

        private List<FieldInfo> GetRequireDependencyFieldInfos(Type type)
        {
            var fieldInfos = type.GetFields
                    (BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(fieldInfo => fieldInfo.GetCustomAttributes<RequireDependency>().Any())
                .ToList();

            if (type.BaseType != null)
            {
                // Recursively walk up the inheritance chain.
                var baseFieldInfos = GetRequireDependencyFieldInfos(type.BaseType);
                fieldInfos.AddRange(baseFieldInfos);
            }

            return fieldInfos;
        }
    }
}