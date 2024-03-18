using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using GIGXR.Platform.UI;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GIGXR.Platform.HMD.UI
{
    public class DebugScreen : BaseUiObject
    {
        private Slate mainSlate;

        private GridLayoutGroup debugOptionHolder;

        private bool debuggerActive = false;

        private HashSet<string> createdButtons = new HashSet<string>();
        private Dictionary<string, (Slate, TextMeshProUGUI)> createdLoggers = new Dictionary<string, (Slate, TextMeshProUGUI)>();

        public Transform SlateTransform
        {
            get
            {
                if(mainSlate != null)
                {
                    return mainSlate.gameObject.transform;
                }

                return null;
            }
        }

        private void OnDestroy()
        {
            if(debuggerActive)
            {
                GIGXR.Platform.Utilities.Logger.NewTaggedMessage -= Logger_NewTaggedMessage;
            }
        }

        private void Logger_NewTaggedMessage(object sender, GIGXR.Platform.Utilities.TagLogMessage e)
        {
            if(createdLoggers.ContainsKey(e.tag))
            {
                createdLoggers[e.tag].Item2.text += $"\n{e.message}";
            }
        }

        /// <summary>
        /// Called via Unity Editor.
        /// </summary>
        [ContextMenu("Start Debugger")]
        public void StartDebugger()
        {
            if (mainSlate != null)
            {
                // Only sub once
                if(!debuggerActive)
                {
                    GIGXR.Platform.Utilities.Logger.NewTaggedMessage += Logger_NewTaggedMessage;

                    debuggerActive = true;
                }

                // TODO Figure out where to put this
                foreach (var loggerInfo in GIGXR.Platform.Utilities.Logger.GetTaggedLoggers())
                {
                    if (!createdButtons.Contains(loggerInfo.Key))
                    {
                        void ShowConsoleLog()
                        {
                            CreateOrShowConsole(loggerInfo);
                        }

                        AddDebugButton(loggerInfo.Value, ShowConsoleLog);

                        createdButtons.Add(loggerInfo.Key);
                    }
                }

                mainSlate.Show(inFrontOfUser: true);
            }
        }

        public void CreateOrShowConsole(KeyValuePair<string, string> typeToShow)
        {
            if (createdLoggers.ContainsKey(typeToShow.Key))
            {
                createdLoggers[typeToShow.Key].Item1.Show(mainSlate.transform);
            }
            else
            {
                var currentSlate = UiBuilder.BuildSlate(typeToShow.Value);

                // Create a black background to lay on top of the default background per Prod specs
                var background = GameObject.Instantiate(UiBuilder.DefaultStyle.slatePureImagePrefab);
                currentSlate.AddContentAsSibling(background);

                var backgroundConnection = background.GetComponent<OmniUIConnection>();
                backgroundConnection.Setup("image", new ImageInfo(Color.black));

                // Normally the mask bounds extends to the top and bottom of the screen, but they should
                // be confined to the background area here
                currentSlate.AdjustMaskPadding(new Vector4(0f, 20f, 0f, 20f));

                var assetTypeComponentDetailText = GameObject.Instantiate(UiBuilder.DefaultStyle.slatePureTextPrefab);
                currentSlate.AddContent(assetTypeComponentDetailText);

                var assetTypeData = assetTypeComponentDetailText.GetComponent<OmniUIConnection>();

                // TODO Update text for console
                assetTypeData.Setup("text", new TextInfo(text: "",
                                                         fontSize: 16.0f,
                                                         textAlignment: TextAlignmentOptions.TopLeft,
                                                         textColor: Color.white));



                currentSlate.AddContent(assetTypeData.gameObject);

                createdLoggers.Add(typeToShow.Key, (currentSlate, assetTypeData.GetTextObject("text")));

                currentSlate.Show(mainSlate.transform);
            }
        }

        public void StopDebugger()
        {
            if (mainSlate != null)
            {
                mainSlate.gameObject.SetActive(false);
            }
        }

        public void SetTitleText(string title)
        {
            if(mainSlate != null)
            {
                mainSlate.SetTitle(title);
            }
        }

        public void AddDebugButton(string buttonName, UnityAction buttonClick)
        {
            if (mainSlate != null)
            {
                mainSlate.AddSlateButton(buttonName, buttonClick, debugOptionHolder.transform);
            }
        }

        public void AddContent(GameObject content)
        {
            if (mainSlate != null)
            {
                mainSlate.AddContent(content);
            }
        }

        protected override void SubscribeToEventBuses()
        {
            if (mainSlate == null)
            {
                mainSlate = UiBuilder.BuildSlate("Main Menu - GigXR Developer Tools");

                debugOptionHolder = UiBuilder.BuildObjectCollection(new Vector2(320.0f, 60.0f), 
                    new Vector2(0.0f, 20.0f), 
                    GridLayoutGroup.Corner.UpperLeft, 
                    GridLayoutGroup.Constraint.FixedColumnCount, 
                    1);

                mainSlate.AddContent(debugOptionHolder.gameObject);

                mainSlate.gameObject.SetActive(false);
            }
        }
    }
}