using System;
using Cysharp.Threading.Tasks;
using GIGXR.GMS.Clients;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.AppEvents.Events.Authentication;
using GIGXR.Platform.Core;
using UnityEngine;

namespace GIGXR.Platform.Networking
{
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// Responsible for maintaining a valid JWT while the user is in the app. If they leave the app
    /// or log out, this will not keep the token valid.
    /// </summary>
    public class ValidJWTHandler : BaseBackgroundHandler
    {
        protected override int MillisecondsDelay { get; }

        private AccountApiClient AccountClient { get; }

        private AppEventBus EventBus { get; }

        public ValidJWTHandler(AppEventBus eventBus, AccountApiClient client, TimeSpan validDuration) : base()
        {
            EventBus = eventBus;
            AccountClient = client;

            // Don't delay by the exact validation duration or the refresh could happen to close to the
            // actual time and be missed by milliseconds
            MillisecondsDelay = (int)(validDuration.TotalMilliseconds * .9);
        }

        public void SetExpirationForToken(TimeSpan validDuration)
        {
            DateTime tokenExpiration = DateTime.UtcNow + validDuration;
            PlayerPrefs.SetString("tokenExpiration", tokenExpiration.ToString(CultureInfo.InvariantCulture));
        }

        protected override async UniTask BackgroundTaskInternalAsync(CancellationToken cancellationToken)
        {
            var tokenExpirationString = PlayerPrefs.GetString("tokenExpiration");

            if (string.IsNullOrEmpty(tokenExpirationString))
            {
                // The user has not logged in yet
                Disable();
            }
            else
            {
                var tokenExpiration = DateTime.Parse(tokenExpirationString, CultureInfo.InvariantCulture);

                if (DateTime.UtcNow <= tokenExpiration)
                {
                    await AccountClient.RefreshTokenAsync();
                }
                else
                {
                    EventBus.Publish(new StartLogOutEvent("Logged out due to inactivity"));
                    PlayerPrefs.DeleteKey("tokenExpiration");
                }
            }
        }
    }
}