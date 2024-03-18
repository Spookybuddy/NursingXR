using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GIGXR.Platform.HMD.UI
{
    public class LogRecord
    {
        public string Stacktrace { get; }
        public LogType LogType { get; }

        public LogRecord(string stack, LogType type)
        {
            Stacktrace = stack;
            LogType = type;
        }
    }

    public class CaptureApplicationLog : MonoBehaviour
    {
        private ConsoleScreen consoleScreen;

        private GridObjectCollection contentTransform;

        private ScrollingObjectCollection scrollingCollection;

        private TextMeshProUGUI stacktraceText;

        // TODO Might be better to save the data and populate an object pool, not sure how that would work with MRTK
        //private List<LogRecord> logRecordList = new List<LogRecord>();

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;

            if(contentTransform != null)
            {
                contentTransform.UpdateCollection();

                scrollingCollection.UpdateContent();

                scrollingCollection.Reset();
            }
        }

        protected void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;

            if (contentTransform != null)
            {
                for (int n = contentTransform.transform.childCount - 1; n >= 0; n--)
                {
                    Destroy(contentTransform.transform.GetChild(n).gameObject);
                }
            }
        }

        public void SetScrollingContent(ConsoleScreen consoleScreen, GridObjectCollection contentTransform, ScrollingObjectCollection scrollingCollection, TextMeshProUGUI stacktraceText)
        {
            this.consoleScreen = consoleScreen;
            this.contentTransform = contentTransform;
            this.scrollingCollection = scrollingCollection;
            this.stacktraceText = stacktraceText;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            //var record = new LogRecord(stackTrace, type);

            //logRecordList.Add(record);
            
            if (consoleScreen != null)
            {
                consoleScreen.CreateRecordObject(logString, stackTrace, type, contentTransform.transform, stacktraceText);

                contentTransform.UpdateCollection();

                scrollingCollection.UpdateContent();
            }
        }
    }
}