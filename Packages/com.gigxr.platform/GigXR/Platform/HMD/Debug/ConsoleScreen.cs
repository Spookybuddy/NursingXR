using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using GIGXR.Platform.UI;

namespace GIGXR.Platform.HMD.UI
{
    public class ConsoleScreen : BaseUiObject
    {
        public GameObject CreateRecordObject(string logText, string stacktrace, LogType logType, Transform parentTransform, TextMeshProUGUI stacktraceText)
        {
            var size = new Vector3(0.2f, 0.03f, 0.01f);

            Color textColor;

            switch (logType)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    textColor = Color.red;
                    break;
                case LogType.Warning:
                    textColor = Color.yellow;
                    break;
                case LogType.Log:
                default:
                    textColor = Color.white;
                    break;
            }

            var renderParent = UiBuilder.BuildQuad(size, null, false);
            renderParent.transform.SetParent(parentTransform, false);

            var record = UiBuilder.BuildText(text: logText, 
                textBoxSize: size,
                textColor: textColor,
                alignmentOptions: TextAlignmentOptions.Left,
                overflowMode: TextOverflowModes.Ellipsis);

            record.transform.SetParent(renderParent.transform, false);

            void ShowRecordData()
            {
                stacktraceText.text = stacktrace;
            }

            var recordButton = UiBuilder.BuildMRTKButton(buttonText: "",
                buttonClick: ShowRecordData,
                buttonSize: size,
                buttonColor: new Color(0,0,0,0));

            recordButton.transform.SetParent(renderParent.transform, false);

            return record;
        }

        public GameObject ScreenObjectFactory()
        {
            var screen = UiBuilder.BuildScreenObject();
            screen.transform.SetParent(transform, false);

            var captureLogs = screen.AddComponent<CaptureApplicationLog>();

            var background = UiBuilder.BuildBackground();
            background.transform.SetParent(screen.transform, false);

            var scrollingCollection = UiBuilder.BuildScrollingPanel(clippingSize: new Vector3(0.2f, 0.05f, 0.05f));
            scrollingCollection.transform.SetParent(screen.transform, false);
            // Position the scrolling up a bit to allow some space for the stack trace text
            scrollingCollection.transform.localPosition += new Vector3(0.0f, 0.0221f, -0.01f);

            var scrollingContentTransform = scrollingCollection.GetComponentInChildren<GridObjectCollection>(true);

            var scrollingContentCollection = scrollingCollection.GetComponentInChildren<ScrollingObjectCollection>(true);

            var stacktraceTextObject = UiBuilder.BuildText(text: "", 
                textBoxSize: new Vector2(0.2f, 0.03f),
                alignmentOptions: TextAlignmentOptions.Left,
                overflowMode: TextOverflowModes.Page); // TODO Stacktrace needs to handle multiple pages/overflow better

            stacktraceTextObject.transform.SetParent(screen.transform, false);
            stacktraceTextObject.transform.localPosition = new Vector3(0.0f, -0.0522f, -0.0005f);

            var stackTraceText = stacktraceTextObject.GetComponentInChildren<TextMeshProUGUI>(true);
            
            captureLogs.SetScrollingContent(this, scrollingContentTransform, scrollingContentCollection, stackTraceText);

            return screen;
        }

        protected override void SubscribeToEventBuses()
        {
        }
    }
}