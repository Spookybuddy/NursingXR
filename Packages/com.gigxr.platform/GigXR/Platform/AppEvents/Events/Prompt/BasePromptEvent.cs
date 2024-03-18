using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.AppEvents.Events.UI
{
    public abstract class BasePromptEvent : IGigEvent<AppEventBus>
    {
        public string HeaderText { get; }

        public string MainText { get; }

        public UIPlacementData PlacementData { get; }

        public PromptManager.WindowStates? WindowWidth { get; }

        public List<ButtonPromptInfo> PromptButtons { get; }

        public BasePromptEvent()
        {
        }

        /// <summary>
        /// Constructor, specifying prompt configuration.
        /// </summary>
        /// <param name="headerText">
        /// The title of the prompt.
        /// </param>
        /// <param name="mainText">
        /// The text to be shown on the prompt.
        /// </param>
        /// <param name="buttons">
        /// A list of <c>ButtonPromptInfo</c> instances, specifying buttons
        /// to appear on the prompt.
        /// </param>
        /// <param name="placementData">
        /// Data to position the prompt, including the prompt's host. The prompt's transform's parent will be set to this
        /// transform, and the prompt will appear in front of this transform with any offsets applied.
        public BasePromptEvent(string headerText, string mainText, List<ButtonPromptInfo> buttons, UIPlacementData placementData = null)
        {
            HeaderText = headerText;
            MainText = mainText;
            PromptButtons = buttons;
            PlacementData = placementData;
        }

        public BasePromptEvent(string headerText, string mainText, List<ButtonPromptInfo> buttons, PromptManager.WindowStates windowWidth, UIPlacementData placementData = null)
        {
            HeaderText = headerText;
            MainText = mainText;
            PromptButtons = buttons;
            WindowWidth = windowWidth;
            PlacementData = placementData;
        }

        public BasePromptEvent(string headerText, string mainText, List<ButtonPromptInfo> buttons, PromptManager.WindowStates windowWidth, Transform hostTransform = null)
        {
            HeaderText = headerText;
            MainText = mainText;
            PromptButtons = buttons;
            WindowWidth = windowWidth;
            PlacementData = new UIPlacementData() { HostTransform = hostTransform };
        }
    }

    public class UIPlacementData
    {
        public Transform HostTransform = null;
        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 RotationOffset = Vector3.zero;
        public GridLayoutOrder ButtonGridLayout = GridLayoutOrder.Horizontal;
        // TODO Should make this automatic to place the grid in the right position, but for right now developers will be responsible for
        // making sure their button contents fit within a window
        public Vector3? ButtonGridLocalPositionOverride = null;
        public Vector2? WindowSize = null;
    }

    /// <summary>
    /// Describes how to place b
    /// </summary>
    public enum GridLayoutOrder
    {
        /// <summary>
        /// Arranges the grid objects along the sides of each other
        /// </summary>
        Horizontal,
        /// <summary>
        /// Arranges the grid objects above one another
        /// </summary>
        Vertical
    }
}