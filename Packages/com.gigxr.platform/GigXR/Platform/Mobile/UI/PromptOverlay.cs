/*
 * THIS CLASS IS OBSOLETE AND UNUSED.
 * 
 * It has been left in the project temporarily as a reference for some
 * functionality that it once supported which has not yet been added to
 * Platform (specifically, download progress).
 */

/*
using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;

using TMPro;
using UnityEngine;

using GIGXR.GMS.Models;

using GIGXR.Platform.UI;
using GIGXR.Platform.Managers;
using GIGXR.Platform.AppEvents.Events.UI;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.Core.DependencyInjection;
using System.Collections.Generic;

namespace GIGXR.Platform.Mobile.UI
{
    using UnityEngine.UI;

    /// <summary>
    /// Generate prompt overlays for mobile devices, via timed prompts or awaited click events
    /// </summary>
    [Obsolete("Use the AppEventBus system with PromptManager to generate prompts across devices")]
    public class PromptOverlay : MonoBehaviour
    {
        public static PromptOverlay Instance;

        /// <summary>
        /// Whether the prompt is showing
        /// </summary>
        public bool isShowing;

        /// <summary>
        /// The text of the prompt
        /// </summary>
        [SerializeField] private TextMeshProUGUI text;

        /// <summary>
        /// The text to display on the accept button
        /// </summary>
        [SerializeField] private TextMeshProUGUI acceptButtonText;

        /// <summary>
        /// The text to display on the decline button
        /// </summary>
        [SerializeField] private TextMeshProUGUI declineButtonText;

        /// <summary>
        /// Reference to the accept button
        /// </summary>
        [SerializeField] private Button acceptButton;

        /// <summary>
        /// Reference to the decline button
        /// </summary>
        [SerializeField] private Button declineButton;

        /// <summary>
        /// The following screen state after button click
        /// </summary>
        //private ScreenState nextScreenState; // TODO mobile prompt update

        /// <summary>
        /// Returns true if the prompt overlay is open
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// Returns true is yes is selected on the awaited prompt
        /// </summary>
        private bool yes;

        private AppEventBus EventBus { get; set; }

        [InjectDependencies]
        public void Construct(AppEventBus eventBus)
        {
            EventBus = eventBus;
        }

        /// <summary>
        /// Initialize from external source as this object is off at runtime
        /// </summary>
        public void Initialize()
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());
            print("Initialize");

            Instance = this;
#if !UNITY_WSA_10_0
            // TODO: CU-250prkv - ContentManager
            // ContentManager.Instance.DownloadComplete += StopActiveCoroutine;
            // ContentManager.Instance.DownloadFailed += InstanceOnDownloadFailed;
#endif
            isShowing = false;
            CloudLogger.LogMethodTrace("End method", MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Task that returns the activity state of the Prompt Overlay
        /// </summary>
        /// <returns>Returns true if prompt is showing</returns>
        private async Task<bool> IsShowing()
        {
            while (isShowing)
            {
                await Task.Delay(10);
            }

            return false;
        }

        /// <summary>
        /// Pass yes = true if the yes button is clicked on the awaited prompt
        /// </summary>
        private void OnYesButtonSelectedAsync()
        {
            acceptButton.onClick.RemoveListener(OnYesButtonSelectedAsync); // -= OnYesButtonSelectedAsync;
            yes = true;
            isOpen = false;
        }

        /// <summary>
        /// Pass yes = false if the no button is clicked on the awaited prompt
        /// </summary>
        private void OnNoButtonSelectedAsync()
        {
            // declineButton.OnClick -= OnNoButtonSelectedAsync;
            declineButton.onClick.RemoveListener(OnNoButtonSelectedAsync);// -= OnNoButtonSelectedAsync;
            yes = false;
            isOpen = false;
        }

        /// <summary>
        /// Set the activity of the prompt
        /// </summary>
        /// <param name="isActive">Whether active or inactive</param>
        private void PromptActivity(bool isActive)
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());
            print($"Prompt activity: {isActive}");
            isOpen = isActive;
            gameObject.SetActive(isActive);
            SetButtonActivity(isActive);
            //ButtonCollection.Instance.EnableOrDisableButtons(!isActive); // TODO mobile prompt update

            CloudLogger.LogMethodTrace("End method", MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Set the button activity of the prompt system
        /// </summary>
        /// <param name="isActive">Whether buttons are active or inactive</param>
        private void SetButtonActivity(bool isActive)
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());
            
            // acceptButton.SetActive(isActive);
            // declineButton.SetActive(isActive);
            
            // TODO - does this work 
            acceptButton.gameObject.SetActive(isActive);
            declineButton.gameObject.SetActive(isActive);

            CloudLogger.LogMethodTrace("End method", MethodBase.GetCurrentMethod());
        }

        #region Downloads

        /// <summary>
        /// The resource id for download
        /// </summary>
        private Guid currentResourceId;

        /// <summary>
        /// The cached coroutine
        /// </summary>
        private IEnumerator coroutine;

        /// <summary>
        /// The WFS between displaying updated download progress
        /// </summary>
        private readonly WaitForSeconds wfs = new WaitForSeconds(1.5f);

        /// <summary>
        /// Start the coroutine for displaying the download progress of the resource
        /// </summary>
        /// <param name="resourceId">The resource in question</param>
        public void StartCoroutineForDownload(Guid resourceId)
        {
            currentResourceId = resourceId;

            print($"{resourceId.ToString()} not found, starting co-routine for download progress");

            // TODO: CU-250prkv - ContentManager
            // ContentManager.Instance.QueueDownload(resourceId);

            //todo Not sure if this is needed
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            EventBus.Publish
            (
                new ShowTimedPromptEvent
                (
                    2000,
                    "Starting Download",
                    null,
                    false,
                    PromptManager.WindowStates.Wide
                )
            );

            //coroutine = ShowProgress();
            StartCoroutine(coroutine);
        }

        /// <summary>
        /// Stop the active coroutine for download progress
        /// </summary>
        /// <param name="resource">The resource downloading</param>
        private void StopActiveCoroutine(Resource resource)
        {
            print("Download complete, stop coroutine");

            EventBus.Publish
            (
                new ShowTimedPromptEvent
                (
                    2000,
                    "Download Complete",
                    null,
                    false,
                    PromptManager.WindowStates.Wide
                )
            );

            if (coroutine == null)
                return;

            StopCoroutine(coroutine);
            coroutine = null;
        }

#if !UNITY_WSA_10_0
        /// <summary>
        /// Show progress for the current download
        /// </summary>
        /// <returns>Wait for seconds</returns>
        private IEnumerator ShowProgress()
        {
            // TODO: CU-250prkv - ContentManager
            // print($"Is {currentResourceId} downloading?: {ContentManager.Instance.IsDownloading(currentResourceId)}");
            // CloudLogger.LogInformation(
            //     $"Is {currentResourceId} downloading?: {ContentManager.Instance.IsDownloading(currentResourceId)}");

            // while (ContentManager.Instance.IsDownloading(currentResourceId))
            // {
            //     CloudLogger.LogDebug($"ShowProgress(): {currentResourceId} downloading still...");
            //
            //     if (ScreenCollection.Instance.activeScreenState == ScreenState.Session)
            //     {
            //         ShowPrompt($"{currentResourceId}"
            //                    + " download in progress: " +
            //                    (ContentManager.Instance.GetDownload(currentResourceId).Progress * 100.0).ToString(
            //                        "0.00") + "%");
            //     }
            //
            //     yield return wfs;
            // }

            yield return new WaitForSeconds(0.25f);
            print("Download not start yet, try again");
            StartCoroutineForDownload(currentResourceId);
        }
#endif
        /// <summary>
        /// Prompt for if the current download fails
        /// </summary>
        /// <param name="resource">The resource to attempt to redownload</param>
        private void InstanceOnDownloadFailed(Resource resource)
        {
            var okButton = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = "Ok",
                        onPressAction = () =>
                        {
                            // TODO: CU-250prkv - ContentManager
                            // ContentManager.Instance.QueueDownload(resource.ResourceId);
                        }
                    },
                    new ButtonPromptInfo()
                    {
                        buttonText = "Cancel",
                        onPressAction = () =>
                        {
                            //ArSceneManager.Instance.ForceEndARSession();
                        }
                    }
                };

            EventBus.Publish
            (
                new ShowPromptEvent
                (
                    "Download failed, retry?",
                    okButton,
                    false,
                    PromptManager.WindowStates.Wide
                )
            );
        }

        #endregion
    }
}
*/
