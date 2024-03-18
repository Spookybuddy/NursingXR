using UnityEngine;
using System.Threading;
using System;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.Interfaces;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Utilities;

namespace GIGXR.Platform
{
    /// <summary>
    /// Connects the forms in the app with the keyboard provided by the OS. On the HMD, this is the MixedRealityKeyboard
    /// class that spawns in front of the user.
    /// </summary>
    public class KeyboardManager : IKeyboardManager, IDisposable
    {        
        private TouchScreenKeyboard keyboard;

        private CancellationTokenSource updateTextCancellationSource;

        private string lastSentText;
        private bool closeOnEnter;

        public event EventHandler<string> OnKeyboardCommit;
        public event EventHandler<string> OnKeyboardStringUpdated;
        public event EventHandler OnKeyboardCancel;
        public event EventHandler OnKeyboardOpened;

        protected AppEventBus EventBus { get; }

        public KeyboardManager(AppEventBus appEventBus)
        {
            EventBus = appEventBus;

            if (updateTextCancellationSource == null)
            {
                updateTextCancellationSource = new CancellationTokenSource();

                // We don't want to await this task as it functions as an update loop
                _ = UpdateTextAsync(updateTextCancellationSource.Token);
            }
        }

        public void ShowKeyboard(string existingText = "",
                                  string textPlaceHolder = "",
                                  TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default,
                                  bool autocorrection = true,
                                  bool multiline = false,
                                  bool secure = false,
                                  bool alert = false,
                                  bool closeOnEnter = true)
        {
            OnKeyboardOpened?.InvokeSafely(this, EventArgs.Empty);

            this.closeOnEnter = closeOnEnter;

            keyboard = TouchScreenKeyboard.Open(existingText, keyboardType, autocorrection, multiline, secure, alert, textPlaceHolder);
        }

        #region IDisposableImplementation

        public void Dispose()
        {
            if (updateTextCancellationSource != null)
            {
                updateTextCancellationSource.Cancel();
                updateTextCancellationSource.Dispose();
                updateTextCancellationSource = null;
            }
        }

        #endregion

        private async UniTask UpdateTextAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(100);

                if (keyboard != null)
                {
                    var keyboardText = keyboard.text;

                    switch (keyboard.status)
                    {
                        case TouchScreenKeyboard.Status.Visible:
                            // Send out the keyboard update event whenever the text changes
                            if(lastSentText != keyboardText)
                            {
                                lastSentText = keyboardText;
                                OnKeyboardStringUpdated?.InvokeSafely(this, keyboardText);
                            }
                            break;
                        case TouchScreenKeyboard.Status.Done:
                            OnKeyboardCommit?.InvokeSafely(this, keyboardText);
                            break;
                        case TouchScreenKeyboard.Status.Canceled:
                            OnKeyboardCancel?.InvokeSafely(this, EventArgs.Empty);
                            break;
                        case TouchScreenKeyboard.Status.LostFocus:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}