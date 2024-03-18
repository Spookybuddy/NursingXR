namespace GIGXR.Platform.HMD.AppEvents.Events
{
    public class SetQrFeedbackTextEvent : QrCodeEvent
    {
        public string FeedbackText { get; }

        public SetQrFeedbackTextEvent(string feedbackText)
        {
            FeedbackText = feedbackText;
        }
    }
}