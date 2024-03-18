using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GIGXR.Platform;
using GIGXR.Platform.Core.FeatureManagement;
using GIGXR.Platform.Interfaces;
using Microsoft.CognitiveServices.Speech;

namespace GIGXR.Dictation
{
    public enum DictationResultStatus
    {
        Success,
        Canceled,
        Error
    }

    public struct DictationResult
    {
        private readonly DictationResultStatus status;
        private readonly string returnedText;

        public DictationResultStatus Status { get { return status; } }
        public string ReturnedText { get { return returnedText; } }

        public DictationResult(DictationResultStatus ResultStatus, string ResultText)
        {
            this.status = ResultStatus;
            this.returnedText = ResultText;
        }
    }

    public class DictationManager : IDictationManager
    {
        public DictationManager(ProfileManager profileManager, IFeatureManager featureManager)
        {
            DictationProvider = new MicrosoftCognitiveServicesDictationProvider(profileManager, featureManager);
        }

        // --- Interfaces
        public IDictationProvider DictationProvider;

        // --- Public Methods:

        /// <summary>
        /// Start the device microphone, capture audio and send it to Azure to convert it to text.
        /// </summary>
        /// <returns></returns>
        public async UniTask<DictationResult> DictateAsync(bool removePunctuation = false)
        {
            try
            {
                DictationResult result = await DictationProvider.Dictate();

                if (result.Status == DictationResultStatus.Success && removePunctuation)
                {
                    string dictatedString = result.ReturnedText;
                    dictatedString =
                        new string(dictatedString.ToCharArray().Where(c => !char.IsPunctuation(c)).ToArray());

                    return new DictationResult(result.Status, dictatedString);
                }
                else
                {
                    return result;
                }
            }
            catch (Exception exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
                return new DictationResult(DictationResultStatus.Error,
                    $"EXCEPTION: DictationManager encountered an error: {exception.Message}");
            }
        }


        /// <summary>
        /// Cancels the currently processing dictation.
        /// </summary>
        public void CancelDictation()
        {
            DictationProvider.CancelDictation();
        }
    }

    public interface IDictationProvider
    {
        UniTask<DictationResult> Dictate();
        void CancelDictation();
    }

    class MicrosoftCognitiveServicesDictationProvider : IDictationProvider
    {
        // --- Constants
        public const string Unrecognised = "Speech could not be recognized";
        public const string DictationDisabled = "Dictation not enabled in this build";
        public const string DictationCanceled = "Dictation in progress was canceled";
        public const string UnhandledResultType = "Dictation encountered an unhandled result";

        // --- Private Variables
        private static bool cancelling;

        private ProfileManager ProfileManager { get; }

        private IFeatureManager FeatureManager { get; }

        public MicrosoftCognitiveServicesDictationProvider(ProfileManager profileManager, IFeatureManager featureManager)
        {
            ProfileManager = profileManager;
            FeatureManager = featureManager;
        }

        /// <summary>
        /// Start the device microphone, capture audio and send it to Azure to convert it to text.
        /// </summary>
        /// <returns></returns>
        public async UniTask<DictationResult> Dictate()
        {
            var dictationEnabled = await FeatureManager.IsEnabledAsync(Platform.Core.Settings.FeatureFlags.Dictation);
            
            if(!dictationEnabled)
            {
                return new DictationResult(DictationResultStatus.Error, DictationDisabled);
            }

            cancelling = false;

            try
            {
                // Create the speech config instance with our key and region.
                SpeechConfig config = SpeechConfig.FromSubscription(ProfileManager.dictationProfile.Key, ProfileManager.dictationProfile.Region);

                // Set the profanity settings.
                config.SetProfanity((ProfanityOption)ProfileManager.dictationProfile.ProfanityOptionSetting);

                // Make sure to dispose the recognizer after use!
                using (SpeechRecognizer recognizer = new SpeechRecognizer(config))
                {
                    // starts a recognizer for 15 seconds, or until silence is detected, whichever comes first.
                    SpeechRecognitionResult result = await recognizer.RecognizeOnceAsync().AsUniTask();

                    if (cancelling)
                    {
                        cancelling = false;
                        return new DictationResult(DictationResultStatus.Canceled, DictationCanceled);
                    }

                    switch (result.Reason)
                    {
                        case ResultReason.RecognizedSpeech:
                            return new DictationResult(DictationResultStatus.Success, result.Text);
                        case ResultReason.NoMatch:
                            return new DictationResult(DictationResultStatus.Error, Unrecognised);
                        case ResultReason.Canceled:
                            CancellationDetails cancellation = CancellationDetails.FromResult(result);
                            return new DictationResult(DictationResultStatus.Canceled,
                                $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}");
                        default:
                            return new DictationResult(DictationResultStatus.Error,
                                $"{UnhandledResultType}. ResultID: {result.ResultId}. Type: {result.Reason.ToString()}. Data: {result.Text}");
                    }
                }
            }
            catch (Exception exception)
            {
                // TODO add back in
                //CloudLogger.LogError(exception);
                return new DictationResult(DictationResultStatus.Error,
                    $"{UnhandledResultType}. Exception Details: {exception.Message}");
            }
        }

        /// <summary>
        /// Cancels the currently processing dictation.
        /// </summary>
        public void CancelDictation()
        {
            cancelling = true;
        }
    }
}