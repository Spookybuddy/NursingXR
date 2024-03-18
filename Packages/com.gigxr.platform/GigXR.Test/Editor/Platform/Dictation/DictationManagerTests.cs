using System.Collections;
using GIGXR.Dictation;
using NUnit.Framework;
using UnityEngine.TestTools;
using GIGXR.Platform.Interfaces;
using Cysharp.Threading.Tasks;

namespace GIGXR.Test.Editor.Dictation
{
    class MockDictationProvider : IDictationProvider
    {
        int waitTimeInMS = 0;
        DictationResultStatus returnStatus;
        string returnText;
        bool canceled = false;

        const string errorText = "Unspecified Dictation Error";
        const string cancelText = "Dictation Canceled";

        public MockDictationProvider(DictationResultStatus returnedStatus, string returnedText,
            int responseWaitTimeInMS = 0)
        {
            this.returnStatus = returnedStatus;
            this.returnText = returnedText;
            this.waitTimeInMS = responseWaitTimeInMS;
        }

        // Allow constructing with specific text to feed back into DictationResults
        public UniTask<DictationResult> Dictate()
        {
            return UniTask.Create(
                async () =>
                {
                    await UniTask.Delay(waitTimeInMS);

                    if (canceled)
                    {
                        return new DictationResult(DictationResultStatus.Canceled, cancelText);
                    }

                    if (returnStatus == DictationResultStatus.Error)
                    {
                        return new DictationResult(DictationResultStatus.Error, errorText);
                    }

                    return new DictationResult(this.returnStatus, this.returnText);
                }
            );
        }

        public void CancelDictation()
        {
            this.canceled = true;
        }
    }

    public class DictationManagerTests
    {
        /* 
         * Tests
         * - If a provider reports success and a text, we get that back
         * - If a provider reports success and a text and we want punctuation gone, it's gone
         * - If a provider reports other than success, it passes to caller with a message
         * - If a provider is asked to cancel before completing, it returns a canceled status
         */

        [UnityTest]
        public IEnumerator DictationManager_SuccessHasSuccessStateAndText() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            int timeToWaitInMS = 0;
            string testText = "TestReturnedText";
            DictationResultStatus testStatus = DictationResultStatus.Success;
            // TODO need to pass in valid ProfileManager data
            IDictationManager testManager = new DictationManager(null, null)
            {
                DictationProvider = new MockDictationProvider(testStatus, testText, timeToWaitInMS)
            };

            // Act
            DictationResult result = await testManager.DictateAsync(false);

            // Assert
            Assert.AreEqual(testStatus, result.Status);
            Assert.AreEqual(testText, result.ReturnedText);
        });

        [UnityTest]
        public IEnumerator DictationManager_SuccessHasSuccessStateTextAndPunctuationRemoved() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            int timeToWaitInMS = 0;
            string testText = "T.e,s!t.R.e.t.u.r.n.e.d.T.e.x.t.";
            string testTextNoPunc = "TestReturnedText";
            DictationResultStatus testStatus = DictationResultStatus.Success;
            // TODO need to pass in valid ProfileManager data
            IDictationManager testManager = new DictationManager(null, null)
            {
                DictationProvider = new MockDictationProvider(testStatus, testText, timeToWaitInMS)
            };

            // Act
            DictationResult result = await testManager.DictateAsync(true);

            // Assert
            Assert.AreEqual(testStatus, result.Status);
            Assert.AreEqual(testTextNoPunc, result.ReturnedText);
        });

        [UnityTest]
        public IEnumerator DictationManager_ErrorHasErrorStateAndText() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            int timeToWaitInMS = 0;
            string testText = "TestReturnedText";
            DictationResultStatus testStatus = DictationResultStatus.Error;
            // TODO need to pass in valid ProfileManager data
            IDictationManager testManager = new DictationManager(null, null)
            {
                DictationProvider = new MockDictationProvider(testStatus, testText, timeToWaitInMS)
            };

            // Act
            DictationResult result = await testManager.DictateAsync(false);

            // Assert
            Assert.AreEqual(testStatus, result.Status);
            Assert.AreNotEqual(testText, result.ReturnedText);
        });

        [UnityTest]
        public IEnumerator DictationManager_CanceledDictationHasCanceledState() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            int timeToWaitInMS = 200;
            string testText = "TestReturnedText";
            DictationResultStatus testStatus = DictationResultStatus.Success;
            // TODO need to pass in valid ProfileManager data
            IDictationManager testManager = new DictationManager(null, null)
            {
                DictationProvider = new MockDictationProvider(testStatus, testText, timeToWaitInMS)
            };

            // Act
            DictationResult result = await testManager.DictateAsync(false);
            testManager.CancelDictation();

            // Assert
            Assert.AreEqual(DictationResultStatus.Canceled, result.Status);
        });
    }
}