namespace GIGXR.Test.Editor.Platform.Core.FeatureManagement
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Core.FeatureManagement;
    using GIGXR.Platform.Core.Settings;
    using NUnit.Framework;
    using System;
    using System.Collections;
    using UnityEngine.TestTools;

    public class BasicEnumFeatureManagerTests
    {
        [UnityTest]
        public IEnumerator BasicEnumFeatureManager_ReturnsFalseWhenNoFlagsAreSet() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            const FeatureFlags featureFlags = FeatureFlags.None;
            var featureManager = new BasicEnumFeatureManager(featureFlags);

            // Act
            var featureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.Dictation);

            // Assert
            Assert.IsFalse(featureEnabled);
        });

        [UnityTest]
        public IEnumerator BasicEnumFeatureManager_ReturnsTrueWhenFlagIsSet() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            const FeatureFlags featureFlags = FeatureFlags.Dictation;
            var featureManager = new BasicEnumFeatureManager(featureFlags);

            // Act
            var featureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.Dictation);

            // Assert
            Assert.IsTrue(featureEnabled);
        });
    }
}