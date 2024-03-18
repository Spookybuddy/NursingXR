using System;
using UnityEngine;

namespace GIGXR.Platform.Interfaces
{
    public interface IKeyboardManager
    {
        event EventHandler<string> OnKeyboardCommit;
        event EventHandler<string> OnKeyboardStringUpdated;
        event EventHandler OnKeyboardCancel;
        event EventHandler OnKeyboardOpened;

        void ShowKeyboard(string existingText = "",
                          string textPlaceHolder = "",
                          TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default,
                          bool autocorrection = true,
                          bool multiline = false,
                          bool secure = false,
                          bool alert = false,
                          bool closeOnEnter = true);
    }
}