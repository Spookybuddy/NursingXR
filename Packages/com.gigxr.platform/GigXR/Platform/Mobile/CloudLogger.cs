using System;
using System.Net.Http;
using System.Reflection;
using UnityEngine;
using GIGXR.GMS.Models;
using GIGXR.Platform.Core.Settings;

namespace GIGXR.Platform.Mobile
{
    /// <summary>
    /// A thin wrapper to send logs to monitoring services in the cloud.
    /// </summary>
    /// <remarks>
    /// At this time it will send to Firebase Crashlytics and Visual Studio App Center.
    /// </remarks>
    public static class CloudLogger
    {
        /// <summary>
        /// The default minimum log level to be captured. This value will be used before the system or user minimum log
        /// levels are available as those are fetched over the network.
        /// </summary>
        public static CloudLogLevel DefaultMinimumLogLevel { get; set; } = CloudLogLevel.Information;

        /// <summary>
        /// The system minimum log level.
        /// </summary>
        public static CloudLogLevel SystemMinimumLogLevel { get; set; } = DefaultMinimumLogLevel;

        /// <summary>
        /// The user minimum log level.
        /// </summary>
        public static CloudLogLevel UserMinimumLogLevel { get; set; } = DefaultMinimumLogLevel;

        /// <summary>
        /// The effective minimum log level. This value is the most verbose between the system and the user log level.
        /// </summary>
        public static CloudLogLevel EffectiveMinimumLogLevel
        {
            get
            {
                if (SystemMinimumLogLevel < UserMinimumLogLevel)
                {
                    return SystemMinimumLogLevel;
                }

                return UserMinimumLogLevel;
            }
        }

        public static async void Init(AuthenticationProfile authenticationProfile)
        {
            var client = new HttpClient();
            var url = $"{authenticationProfile.ApiUrl()}/healthcheck/gig-mobile-minimum-log-level";
            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var json = JsonUtility.FromJson<SuccessResponse<GigMobileLogLevelView>>(responseContent);
                SystemMinimumLogLevel = json.data.gigMobileMinimumLogLevel;
                LogInformation($"Setting system log level to {SystemMinimumLogLevel}");
            }
            catch (Exception exception)
            {
                LogWarning("Unable to fetch MinimumLogLevel!");
                LogError(exception);
            }
        }

        /// <summary>
        /// Sends a message to cloud logging services.
        /// </summary>
        /// <param name="logLevel">The log level of the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Log(CloudLogLevel logLevel, string message)
        {
            if (logLevel == CloudLogLevel.None)
            {
                return;
            }

            if (logLevel < EffectiveMinimumLogLevel)
            {
                return;
            }

            // Turn on local logging for development here.
            // Debug.Log(message);

            // Firebase Crashlytics
            //Crashlytics.Log($"{message}");

            // Visual Studio App Center => Azure Application Insights
            //Analytics.TrackEvent($"{message}");
        }

        /// <summary>
        /// Sends an Exception to cloud logging services.
        /// </summary>
        /// <param name="logLevel">The log level of the message.</param>
        /// <param name="exception">The Exception to send.</param>
        public static void Log(CloudLogLevel logLevel, Exception exception)
        {
            if (logLevel == CloudLogLevel.None)
            {
                return;
            }

            if (logLevel < EffectiveMinimumLogLevel)
            {
                return;
            }

            // Turn on local logging for development here.
            // Debug.LogException(exception);

            // Firebase Crashlytics
            //Crashlytics.LogException(exception);

            // Visual Studio App Center => Azure Application Insights
            //Crashes.TrackError(exception);
        }

        public static void LogTrace(string message)
            => Log(CloudLogLevel.Trace, message);

        public static void LogTrace(System.Exception exception)
            => Log(CloudLogLevel.Trace, exception);


        public static void LogDebug(string message)
            => Log(CloudLogLevel.Debug, message);

        public static void LogDebug(System.Exception exception)
            => Log(CloudLogLevel.Debug, exception);


        public static void LogInformation(string message)
            => Log(CloudLogLevel.Information, message);

        public static void LogInformation(System.Exception exception)
            => Log(CloudLogLevel.Information, exception);


        public static void LogWarning(string message)
            => Log(CloudLogLevel.Warning, message);

        public static void LogWarning(System.Exception exception)
            => Log(CloudLogLevel.Warning, exception);


        public static void LogError(string message)
            => Log(CloudLogLevel.Error, message);

        public static void LogError(System.Exception exception)
            => Log(CloudLogLevel.Error, exception);


        public static void LogCritical(string message)
            => Log(CloudLogLevel.Critical, message);

        public static void LogCritical(System.Exception exception)
            => Log(CloudLogLevel.Critical, exception);


        public static void LogNone(string message)
            => Log(CloudLogLevel.None, message);

        public static void LogNone(System.Exception exception)
            => Log(CloudLogLevel.None, exception);


        public static void LogMethodTrace(string message, MethodBase methodBase)
            => Log(CloudLogLevel.Trace, $"{message}: {methodBase.ReflectedType?.Name}.{methodBase.Name}");


        [Serializable]
        public class GigMobileLogLevelView
        {
            public CloudLogLevel gigMobileMinimumLogLevel;
        }
    }

    /// <summary>
    /// Used to define the log level of logging events sent to cloud providers.
    /// </summary>
    /// <remarks>
    /// Levels inspired by:
    /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-3.1
    /// </remarks>
    public enum CloudLogLevel
    {
        /// <summary>
        /// Logs that contain the most detailed messages. These messages may contain sensitive application data. These
        /// messages are disabled by default and should never be enabled in a production environment.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Logs that are used for interactive investigation during development. These logs should primarily contain
        /// information useful for debugging and have no long-term value.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Logs that track the general flow of the application. These logs should have long-term value.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the
        /// application execution to stop.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a
        /// failure in the current activity, not an application-wide failure.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires
        /// immediate attention.
        /// </summary>
        Critical = 5,

        /// <summary>
        /// Not used for writing log messages. Specifies that a logging category should not write any messages.
        /// </summary>
        None = 6,
    }
}