using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GIGXR.GMS.Models
{
    public enum ResourceType
    {
        Hcap,
        AssetBundle,
        Texture,
    }

    [Serializable]
    public class ClientAppManifest
    {
        public ClientAppManifest(
            Dictionary<string, ResourceCustomProperty> customProperties,
            Dictionary<Guid, Resource> resourcesById)
        {
            CustomProperties = customProperties;
            ResourcesById = resourcesById;
        }

        public Dictionary<string, ResourceCustomProperty> CustomProperties { get; }
        public Dictionary<Guid, Resource> ResourcesById { get; }
    }

    public static class ClientAppManifestExtensions
    {
        [CanBeNull]
        public static Resource GetResourceById(this ClientAppManifest manifest, Guid resourceId)
        {
            if (manifest.ResourcesById.TryGetValue(resourceId, out var resource))
            {
                return resource;
            }

            return null;
        }
    }

    [Serializable]
    public class Resource
    {
        public Resource(
            Guid resourceId,
            ResourceType type,
            string name,
            string description,
            List<string> tags,
            ResourceUrls urls,
            Dictionary<string, ResourceProperty> properties,
            Dictionary<string, string> metadata)
        {
            ResourceId = resourceId;
            Type = type;
            Name = name;
            Description = description;
            Tags = tags;
            Urls = urls;
            Properties = properties;
            Metadata = metadata;
        }

        public Guid ResourceId { get; }
        public ResourceType Type { get; }
        public string Name { get; }
        public string Description { get; }
        public List<string> Tags { get; }
        public ResourceUrls Urls { get; }
        public Dictionary<string, ResourceProperty> Properties { get; }
        public Dictionary<string, string> Metadata { get; }
    }

    public static class ResourceExtensions
    {
        [CanBeNull]
        public static Uri GetUriForPlatform(this Resource resource)
        {
            Uri url = null;

#if UNITY_WSA_10_0
            url = resource.Urls.Wsa;
#elif UNITY_ANDROID
            url = resource.Urls.Android;
#elif UNITY_IOS
            url = resource.Urls.IOS;
#endif

            return url ?? resource.Urls?.Default;
        }
    }

    public class ResourceUrls
    {
        public ResourceUrls(
            Uri wsa,
            Uri android,
            Uri iOS,
            Uri @default)
        {
            Wsa = wsa;
            Android = android;
            IOS = iOS;
            Default = @default;
        }

        public Uri Wsa { get; }
        public Uri Android { get; }
        public Uri IOS { get; }
        public Uri Default { get; }
    }

    public class ResourceProperty
    {
        public ResourceProperty(
            string type,
            string value,
            int index,
            string displayName)
        {
            Type = type;
            Value = value;
            Index = index;
            DisplayName = displayName;
        }

        public string Type { get; }
        public string Value { get; }
        public int Index { get; }
        public string DisplayName { get; }
    }

    public class ResourceCustomProperty
    {
        public ResourceCustomProperty(string type)
        {
            Type = type;
        }

        public string Type { get; }
    }
}