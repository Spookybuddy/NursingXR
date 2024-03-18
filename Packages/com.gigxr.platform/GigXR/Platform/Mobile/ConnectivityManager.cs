using System;
using System.Collections;
using System.Diagnostics;

using TMPro;
using UnityEngine;
using UnityEngine.Networking;

using GIGXR.GMS.Models;
using GIGXR.Platform.Mobile.Utilities;

using GIGXR.Platform.Mobile.UI;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Managers;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.AppEvents;
using System.Collections.Generic;
using GIGXR.Platform.AppEvents.Events.UI;
using GIGXR.Platform.Core.Settings;

namespace GIGXR.Platform.Mobile
{
    // TODO replace coroutines with appropriate event subscriptions, remove from scene, remove Monobehavior base

    /// <summary>
    /// ConnectivityManager is used to run a coroutine that periodically sends HTTP requests to the GIGXR servers to see if
    /// they are accessible. It also controls a basic screen, `NoConnectivityScreen`, that is shown when the servers cannot
    /// be reached.
    /// </summary>
    [DisallowMultipleComponent]
    public class ConnectivityManager : MonoBehaviour
    {
        private static ConnectivityManager _instance;
    
        /// <summary>
        /// The singleton instance of ConnectivityManager.
        /// </summary>
        public static ConnectivityManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ConnectivityManager>();
                }
    
                return _instance;
            }
        }
    
        /// <summary>
        /// This event is triggered when connectivity is lost.
        /// </summary>
        public static event Action ConnectivityLost;
    
        /// <summary>
        /// This event is triggered when connectivity has been restored.
        /// </summary>
        public static event Action ConnectivityRestored;
    
        /// <summary>
        /// This is the delay between successful pings to the GIGXR servers.
        /// </summary>
        public TimeSpan PingDelay { get; } = TimeSpan.FromSeconds(5);
    
        /// <summary>
        /// This is the number of failed retries before connectivity is considered lost.
        /// </summary>
        public int FailedRetryThreshold { get; } = 2;
    
        /// <summary>
        /// Whether there is connectivity to the GIGXR servers.
        /// </summary>
        public bool HasConnectivity { get; private set; } = true;
        
        /// <summary>
        /// The status text to be displayed when there is no connectivity. Attached in the editor.
        /// </summary>
        public TMP_Text statusText;
    
        /// <summary>
        /// The URI to periodically check access to.
        /// </summary>
        private Uri _connectivityUri;
    
        /// <summary>
        /// The back-off strategy used for retry attempts.
        /// </summary>
        private ExponentialBackOff _backOff;
    
        /// <summary>
        /// A reference to the check connectivity coroutine. It is stored here so we can stop the coroutine.
        /// </summary>
        private Coroutine _checkConnectivityCoroutine;
    
        /// <summary>
        /// The next scheduled connectivity check.
        /// </summary>
        private DateTime _nextConnectivityCheck;
    
        /// <summary>
        /// A Stopwatch used to prevent Update() from running too frequently.
        /// </summary>
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
    
        /// <summary>
        /// How frequently Update() is allowed to run in milliseconds.
        /// </summary>
        private const int MinimumUpdateDelayInMilliseconds = 1000;

        /// <summary>
        /// The screen to be displayed when there is no connectivity. Attached in the editor.
        /// </summary>
        // private GameObject noConnectivityScreen;
        private NoConnectivityScreen noConnectivityScreen;

        private INetworkManager NetworkManager { get; set; }

        private AppEventBus EventBus { get; set; }

        private ProfileManager ProfileManager { get; set; }

        [InjectDependencies]
        public void Construct(AppEventBus eventBus, INetworkManager networkManager, ProfileManager profileManager)
        {
            NetworkManager = networkManager;
            EventBus = eventBus;
            ProfileManager = profileManager;

            var apiUri = new Uri($"{ProfileManager.authenticationProfile.ApiUrl()}/");
            
            _connectivityUri = new Uri(apiUri, "healthcheck");
            
            CloudLogger.LogInformation($"cm - ConnectivityUri: {_connectivityUri}");
        }
        
        private void Awake()
        {
            _backOff = new ExponentialBackOff(new ExponentialBackOffConfig
            {
                MaxRetryCount = int.MaxValue,
                MaxRetryDuration = TimeSpan.MaxValue,
            });

            _updateStopwatch.Start();
        }
    
        private void Start()
        {
            noConnectivityScreen = FindObjectOfType<NoConnectivityScreen>();
            
            StartCheckConnectivityCoroutine();
        }
    
        private void Update()
        {
            if (_updateStopwatch.ElapsedMilliseconds < MinimumUpdateDelayInMilliseconds)
            {
                return;
            }
    
            _updateStopwatch.Restart();
    
            if (HasConnectivity)
            {
                return;
            }
    
            UpdateStatusText();
        }
    
        private IEnumerator OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || NetworkManager == null)
            {
                yield break;
            }
    
            // Check connectivity on application focus.
            CheckConnectivityNow();
        }
    
        private void UpdateStatusText()
        {
            var nextCheck = _nextConnectivityCheck.Subtract(DateTime.UtcNow).TotalSeconds;
            statusText?.SetText(nextCheck >= 1.0f
                ? $"Trying again in {(int) nextCheck} seconds."
                : "Trying again now...");
            _updateStopwatch.Restart();
        }
    
        public void CheckConnectivityNow()
        {
            StopCheckConnectivityCoroutine();
            StartCheckConnectivityCoroutine();
            UpdateStatusText();
        }
    
        private void StartCheckConnectivityCoroutine()
        {
            if(_checkConnectivityCoroutine == null)
            {
                _checkConnectivityCoroutine = StartCoroutine(CheckConnectivity());
            }
        }
    
        private void StopCheckConnectivityCoroutine()
        {
            if (_checkConnectivityCoroutine != null)
            {
                StopCoroutine(_checkConnectivityCoroutine);
                _checkConnectivityCoroutine = null;
            }
        }
    
        private IEnumerator CheckConnectivity()
        {
            if(_connectivityUri == null)
                yield break;
                
            _nextConnectivityCheck = DateTime.UtcNow;
    
            var request = UnityWebRequest.Get(_connectivityUri);
            yield return request.SendWebRequest();
    
            if (request.isNetworkError || request.isHttpError)
            {
                CloudLogger.LogInformation("cm - CheckConnectivity failed - network error");
                yield return FailedConnectivityCheck();
                yield break;
            }
    
            if (request.responseCode < 200 || request.responseCode > 299)
            {
                CloudLogger.LogInformation("cm - CheckConnectivity failed - bad response code");
                yield return FailedConnectivityCheck();
                yield break;
            }
    
            var body = request.downloadHandler.text;
            var json = JsonUtility.FromJson<SuccessResponse<string>>(body);
            if (json == null)
            {
                CloudLogger.LogInformation("cm - CheckConnectivity failed - no JSON body");
                yield return FailedConnectivityCheck();
                yield break;
            }
    
            if (json.data != "alive")
            {
                CloudLogger.LogInformation("cm - CheckConnectivity failed - non-alive JSON body");
                yield return FailedConnectivityCheck();
                yield break;
            }
    
            CloudLogger.LogDebug("cm - CheckConnectivity success!");
    
            yield return SuccessfulConnectivityCheck();
        }

        private void ConnectionLostPrompt()
        {
            var okButton = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = "Ok",
                        onPressAction = () =>
                        {
                            SetConnectivityActivity();
                        }
                    },
                    new ButtonPromptInfo()
                    {
                        buttonText = "Cancel",
                        onPressAction = () =>
                        {
                            NetworkManager.LeaveRoomAsync();
                        }
                    }
                };

            EventBus.Publish
            (
                new ShowPromptEvent
                (
                    $"Connection Lost",
                    okButton,
                    PromptManager.WindowStates.Wide
                )
            );
        }
    
        private IEnumerator FailedConnectivityCheck()
        {
            if (HasConnectivity && _backOff.RetryCount >= FailedRetryThreshold)
            {
                CloudLogger.LogInformation(
                    $"cm - Retry count[{_backOff.RetryCount}] met threshold[{FailedRetryThreshold}], triggering {nameof(ConnectivityLost)} event!");
                HasConnectivity = false;
                ConnectivityLost?.Invoke();
                //Set prompt for connection lost before no connectivity screen
                ConnectionLostPrompt();
            }

            // UniWebViewController.Instance.HideWebview();
            //ScreenCollection.Instance.SetScreenState(ScreenState.Connectivity); // TODO handle connectivity checks mobile

            var nextDelay = _backOff.NextDelay();
            if (nextDelay == null)
            {
                CloudLogger.LogInformation("cm - No more connectivity retries, giving up! :(");
                yield break;
            }
    
            _nextConnectivityCheck = DateTime.UtcNow.Add(nextDelay.Value);
            CloudLogger.LogDebug($"cm - Trying again in {(int) nextDelay.Value.TotalSeconds} seconds.");
            yield return new WaitForSeconds((int) nextDelay.Value.TotalSeconds);
            StartCheckConnectivityCoroutine();
        }

        private void SetConnectivityActivity()
        {
            // bool noConnectivityScreenActive = !noConnectivityScreen.activeSelf;
            // noConnectivityScreen.SetActive(noConnectivityScreenActive);
            //ScreenCollection.Instance.SetScreenState(ScreenState.Connectivity); // TODO handle connectivity checks mobile
            noConnectivityScreen.Show(HasConnectivity);
        }

        private IEnumerator SuccessfulConnectivityCheck()
        {
            if (!HasConnectivity)
            {
                HasConnectivity = true;
                ConnectivityRestored?.Invoke();
                SetConnectivityActivity();
            }
    
            // TODO: Move this somewhere else, probably to a UI controller/view that listens to these events
            // if (!UniWebViewController.Instance.IsVisible && !ServiceLocator.NetworkManager.InRoom)
            // {
            //     UniWebViewController.Instance.LoadAndShowWebview();
            // }

            _backOff.Reset();
    
            CloudLogger.LogDebug($"cm - SuccessfulConnectivityCheck - waiting for {PingDelay}");
            _nextConnectivityCheck = DateTime.UtcNow.Add(PingDelay);
            yield return new WaitForSeconds((int) PingDelay.TotalSeconds);
            StartCheckConnectivityCoroutine();
        }
    }
}
