using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.Core.DependencyInjection;

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// Core class for any ATC like class that needs to access the Asset Mediator
    /// and the property values easily.
    /// </summary>
    public abstract class AssetTypeComponent : MonoBehaviour
    {
        public IAssetMediator AttachedAssetMediator
        {
            get
            {
                if (attachedInteractable == null)
                {
                    attachedInteractable = GetComponent<IAssetMediator>();
                }

                return attachedInteractable;
            }
        }

        protected IAssetMediator attachedInteractable;

        protected Dictionary<string, Action<AssetPropertyChangeEventArgs>> propertyChangeDelegates
               = new Dictionary<string, Action<AssetPropertyChangeEventArgs>>();

        protected IGigAssetManager AssetManager { get; private set; }

        [InjectDependencies]
        public void Construct(IGigAssetManager assetManager)
        {
            AssetManager = assetManager;

            AssetTypeDependenciesConstructed();
        }

        protected virtual void AssetTypeDependenciesConstructed()
        {
            // Base classes can choose to implement this or not
        }

        protected void AddAction(string propertyName, Action<AssetPropertyChangeEventArgs> action)
        {
            propertyChangeDelegates.Add(propertyName, action);

            RegisterPropertyChange(propertyName, action);
        }

        protected void RegisterPropertyChange
        (
            string assetPropertyName,
            Action<AssetPropertyChangeEventArgs> lambda
        )
        {
            attachedInteractable.RegisterPropertyChange(assetPropertyName, lambda);
        }

        protected void RegisterAllPropertyChangeAttributes(Action<Type> additionalAction = null, IAssetMediator givenInteractable = null)
        {
            // When registering all property changes from a subclass, it's possible that this isn't assigned yet so let it be a 
            // short cut since the data is already known
            if(attachedInteractable == null && givenInteractable != null)
            {
                attachedInteractable = givenInteractable;
            }

            Type type = GetType();

            while ((!type.IsGenericType ||
                    type.GetGenericTypeDefinition() != typeof(BaseAssetTypeComponent<>)) &&
                    type != typeof(MonoBehaviour) &&
                    type != typeof(LocalAssetTypeComponent) &&
                    type != typeof(AssetTypeComponent))
            {
                additionalAction?.Invoke(type);

                RegisterAllPropertyChangeAttributeForType(type);

                // Make sure to go down the ATC base types to handle inheritance within ATCs
                type = type.BaseType;
            }
        }

        private void RegisterAllPropertyChangeAttributeForType(Type type)
        {
            var methodAttributes = type
                                   .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                   .Where(method => method.GetCustomAttribute<RegisterPropertyChangeAttribute>() != null);

            if (methodAttributes != null && methodAttributes.Count() != 0)
            {
                foreach (var methodInfo in methodAttributes)
                {
                    var attribute = methodInfo.GetCustomAttribute<RegisterPropertyChangeAttribute>();

                    Action<AssetPropertyChangeEventArgs> action = (Action<AssetPropertyChangeEventArgs>)Delegate.CreateDelegate(typeof(Action<AssetPropertyChangeEventArgs>), this, methodInfo);

                    AddAction(attribute.PropertyName, action);
                }
            }
        }

        protected void UnregisterAllPropertyChangeAttributes()
        {
            // unregister property change handlers
            foreach (var method in propertyChangeDelegates)
            {
                UnregisterPropertyChange(method.Key);
            }

            propertyChangeDelegates.Clear();
        }

        protected void UnregisterPropertyChange(string assetPropertyName)
        {
            attachedInteractable.UnregisterPropertyChange(assetPropertyName);
        }
    }
}