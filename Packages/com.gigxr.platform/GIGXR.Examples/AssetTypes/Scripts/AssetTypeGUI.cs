using System;
using UnityEngine;
namespace GIGXR.Platform
{
    using GIGXR.Platform.Scenarios.GigAssets;
    using GIGXR.Platform.Scenarios.GigAssets.Data;

    /// <summary>
    /// Add this class to any object in the scene and fill in any of the
    /// names of interests, then press the related GUI button.
    /// </summary>
    public class AssetTypeGUI : MonoBehaviour
    {
        public string methodName;
        public string stateName;
        public string eventName;
        public AssetMediator interactable;
        public Rect windowRect = new Rect(20, 100, 150, 350);
        public Vector2 buttonSize = new Vector2(135, 20);
        public Vector3 lerpPosition;
        public float lerpDuration;

        public string propertyName;
        public Color colorProperty;
        public bool boolValue;
        public float floatValue;
        public Vector3 vectorProperty;

        void OnGUI()
        {
            windowRect = GUI.Window(nameof(AssetTypeGUI).GetHashCode(), windowRect, SetupWindow, "Asset Window");
        }

        // Make the contents of the window
        void SetupWindow(int windowID)
        {
            if (GUI.Button(new Rect(10, 20, buttonSize.x, buttonSize.y), "Call Method"))
            {
                interactable.CallAssetMethod(methodName);
            }

            if (GUI.Button(new Rect(10, 50, buttonSize.x, buttonSize.y), "Get State"))
            {
                var test = interactable.GetAssetState<object>(stateName);

                Debug.Log($"TestGUI saw {test[0].Item1}");
            }

            if (GUI.Button(new Rect(10, 80, buttonSize.x, buttonSize.y), "Register Event"))
            {
                interactable.RegisterWithAssetEvent(this, eventName, LogEvent);
            }

            if (GUI.Button(new Rect(10, 110, buttonSize.x, buttonSize.y), "Unegister Event"))
            {
                interactable.UnregisterWithAssetEvent(eventName, LogEvent);
            }

            if (GUI.Button(new Rect(10, 140, buttonSize.x, buttonSize.y), "Lerp position"))
            {
                //Lerp(Vector3 targetPosition, float duration)
                interactable.CallAssetMethod("Lerp", new object[] { lerpPosition, lerpDuration });
            }

            if (GUI.Button(new Rect(10, 170, buttonSize.x, buttonSize.y), "Set Color Property"))
            {
                interactable.SetAssetProperty(propertyName, colorProperty);
            }

            if (GUI.Button(new Rect(10, 200, buttonSize.x, buttonSize.y), "Set Bool Property"))
            {
                interactable.SetAssetProperty(propertyName, boolValue);
            }

            if (GUI.Button(new Rect(10, 230, buttonSize.x, buttonSize.y), "Set Float Property"))
            {
                interactable.SetAssetProperty(propertyName, floatValue);
            }

            if (GUI.Button(new Rect(10, 260, buttonSize.x, buttonSize.y), "Set Vector3 Property"))
            {
                interactable.SetAssetProperty(propertyName, vectorProperty);
            }

            if (GUI.Button(new Rect(10, 290, buttonSize.x, buttonSize.y), "Get Property"))
            {
                var property = interactable.GetAssetProperty(propertyName);

                Debug.Log($"{propertyName} has value {property}");
            }

            if (GUI.Button(new Rect(10, 320, buttonSize.x, buttonSize.y), "Get Asset Data"))
            {
                var assetData = interactable.GetAssetData<BaseAssetData>(propertyName);

                Debug.Log($"{propertyName} has assetData {assetData}");
            }

            GUI.DragWindow();
        }

        public void LogEvent(object sender, EventArgs e)
        {
            Debug.Log($"TestGUI just saw event from {sender} with args {e}");
        }
    }
}