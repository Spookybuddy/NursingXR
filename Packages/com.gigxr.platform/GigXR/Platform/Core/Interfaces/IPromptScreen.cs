using GIGXR.Platform.AppEvents;
using GIGXR.Platform.AppEvents.Events.UI;
using GIGXR.Platform.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Interfaces
{
    public interface IPromptScreen
    {
        GameObject SelfGameObject { get; }

        void SetDependencies(AppEventBus eventBus);

        void SetHeaderText(string header);

        void SetWindowText(string message);

        void SetWindowSize(int windowWidth, int backgroundHeight);

        void CreateButtons(List<ButtonPromptInfo> newButtons, Vector2? size = null);

        void SetButtonLayout(UIPlacementData transformData);

        void AdjustGridTransform(UIPlacementData transformData);

        void RemoveScreen();

        void PlaySFX();
    }
}