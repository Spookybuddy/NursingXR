using GIGXR.Platform.AppEvents.Events.Session;
using GIGXR.Platform.Core.User;
using GIGXR.Platform.UI;
using Microsoft.MixedReality.Toolkit.UI;

public class PreferencesSceenObject : BaseUiObject
{
    public Interactable headToggle;

    public Interactable handToggle;

    public Interactable localHandToggle;

    public Interactable nametagToggle;

    /// <summary>
    /// Called via Unity Editor
    /// </summary>
    public void ToggleAvatarHeads()
    {
        UserRepresentations.ToggleAllAvatarHeadState();
    }

    /// <summary>
    /// Called via Unity Editor
    /// </summary>
    public void ToggleAvatarHands()
    {
        UserRepresentations.ToggleAllAvatarHandState();
    }

    /// <summary>
    /// Called via Unity Editor
    /// </summary>
    public void ToggleLocalHands()
    {
        UserRepresentations.ToggleLocalHandState();
    }

    /// <summary>
    /// Called via Unity Editor
    /// </summary>
    public void ToggleNameTags()
    {
        UserRepresentations.ToggleAllNametagState();
    }

    private void OnJoinedSessionEvent(JoinedSessionEvent @event)
    {
        headToggle.IsToggled = UserRepresentations.HeadsEnabled;
        handToggle.IsToggled = UserRepresentations.HandsEnabled;
        localHandToggle.IsToggled = UserRepresentations.LocalHandsEnabled;
        nametagToggle.IsToggled = UserRepresentations.NameTagsEnabled;
    }

    protected override void SubscribeToEventBuses()
    {
        EventBus.Subscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
    }
}
