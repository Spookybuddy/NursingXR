namespace GIGXR.Platform.Core.FeatureManagement
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Core.Settings;
    using System;

    public interface IFeatureManager
    {
        /// <summary>
        /// Returns whether the specified feature is enabled.
        /// </summary>
        /// <remarks>
        /// This method is async in case future implementations would want to do more dynamic
        /// feature checking that might require interacting with an external API such as the GMS.
        /// 
        /// For example, there could be a GmsFeatureManager that calls the API to check whether a
        /// User has opted in or paid for a specific feature. If this method was sync that would not
        /// be as possible.
        /// </remarks>
        /// <param name="feature">The specified feature to check if it is enabled.</param>
        /// <returns>A bool representing if the feature is enabled.</returns>
        UniTask<bool> IsEnabledAsync(FeatureFlags feature);

        bool IsEnabled(FeatureFlags feature);

        event EventHandler<FeatureChangeEventArgs> RuntimeFeatureChanged;

        void AddRuntimeFeature(FeatureFlags feature);

        void RemoveRuntimeFeature(FeatureFlags feature);
    }
}