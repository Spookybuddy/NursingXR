namespace GIGXR.Platform.Core.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.UIElements;
#endif
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Holds data that is set in the Editor via Edit/Project Settings/GIGXR Settings that are accessible at runtime.
    /// </summary>
    public class GIGXRSettings
    {
#if UNITY_EDITOR
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/GIGXR Settings", SettingsScope.Project)
            {
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = ProfileManager.GetSerializedSettings();

                    var scrollView = new ScrollView();
                    rootElement.Add(scrollView);

                    // TODO logo is a bit large for the screen
                    var logo = new Image()
                    {
                        image = (Texture2D)Resources.Load
                            ("Branding/GIGXR_Logo_Green", typeof(Texture2D))
                    };

                    scrollView.Add(logo);

                    foreach (var field in GetVisibleSerializedFields
                        (settings.targetObject.GetType()))
                    {
                        var flaggedFeature = field.GetCustomAttribute
                            (typeof(FeatureFlagAttribute)) as FeatureFlagAttribute;

                        // If the feature is not flagged, then it will always be displayed
                        if (flaggedFeature == null)
                        {
                            var serializedProperty = settings.FindProperty(field.Name);

                            var propertyField = new PropertyField(serializedProperty);

                            scrollView.Add(propertyField);
                        }
                        else
                        {
                            // Grab the FeatureFlag enum profile directly
                            var o = settings.FindProperty
                                (
                                    $"{nameof(ProfileManager.FeatureFlagsProfile)}.{nameof(FeatureFlagsProfile.FeatureFlags)}"
                                );

                            var featureFlags = (FeatureFlags)o.enumValueIndex;

                            var hasFlaggedFeature = featureFlags.HasFlag
                                (flaggedFeature.FlaggedFeatures);

                            // Only display the profile if the feature is enabled
                            if (hasFlaggedFeature)
                            {
                                var serializedProperty = settings.FindProperty(field.Name);

                                var propertyField = new PropertyField(serializedProperty);

                                scrollView.Add(propertyField);
                            }
                        }
                    }

                    rootElement.Bind(settings);

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                },
                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Profile Manager", "Profile" })
            };

            return provider;
        }
#endif

        public static FieldInfo[] GetVisibleSerializedFields(Type T)
        {
            List<FieldInfo> infoFields = new List<FieldInfo>();

            var publicFields = T.GetFields(BindingFlags.Instance | BindingFlags.Public);

            for (int i = 0; i < publicFields.Length; i++)
            {
                if (publicFields[i].GetCustomAttribute<HideInInspector>() == null)
                {
                    infoFields.Add(publicFields[i]);
                }
            }

            var privateFields = T.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            for (int i = 0; i < privateFields.Length; i++)
            {
                if (privateFields[i].GetCustomAttribute<SerializeField>() != null)
                {
                    infoFields.Add(privateFields[i]);
                }
            }

            return infoFields.ToArray();
        }
    }
}