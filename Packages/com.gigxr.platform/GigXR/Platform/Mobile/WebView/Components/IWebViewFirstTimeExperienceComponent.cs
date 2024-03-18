namespace GIGXR.Platform.Mobile.WebView.Components
{
    /// <summary>
    /// An interface for accessing the first time experience state.
    /// </summary>
    public interface IWebViewFirstTimeExperienceComponent
    {
        bool SkipFirstTimeExperience { get; set; }
    }
}