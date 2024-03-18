using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Core.Settings
{
    /// <summary>
    /// An attribute that can be attached to fields on the ProfileManager that will allow them
    /// to be enabled/disabled based on their feature setting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class FeatureFlagAttribute : Attribute
    {
        public FeatureFlags FlaggedFeatures;

        public FeatureFlagAttribute(FeatureFlags flag)
        {
            FlaggedFeatures = flag;
        }
    }
}