using System;
using UnityEngine;

namespace GIGXR.Platform
{
    // Unity Editor modes, e.g. Editor.isPlaying
    public enum UnityPlayerModes
    {
        EditMode,
        PlayMode
    }

    public class ShowInPlayerModeAttribute : PropertyAttribute
    {
        public readonly UnityPlayerModes ShowInThisMode;

        public ShowInPlayerModeAttribute(UnityPlayerModes showInPlayMode)
        {
            this.ShowInThisMode = showInPlayMode;
        }
    }
}