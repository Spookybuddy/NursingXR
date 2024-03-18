using System;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GIGXR.Test.Editor.Platform.WebView.EventBus.WebViewToUnity.Events
{
    public class CreateSessionWebViewToUnityEventTests
    {
        [Test]
        public void CreateSessionWebViewToUnityEvent_DeserializesCorrectly()
        {
            // Arrange
            const string sessionId = "14560acf-76d0-407b-93b0-1afd84bcd07a";
            var json = $@"{{""sessionId"": ""{sessionId}""}}";
            var serializer = new CreateSessionWebViewEventSerializer();

            // Act
            var @event = serializer.Deserialize(json);

            // Assert
            Assert.AreEqual(sessionId, @event.SessionId.ToString());
        }

        [Test]
        public void CreateSessionWebViewToUnityEvent_ThrowsForInvalidJsonContent()
        {
            // Arrange
            const string sessionId = "invalid";
            var json = $@"{{""sessionId"": ""{sessionId}""}}";
            var serializer = new CreateSessionWebViewEventSerializer();

            // Act
            void DeserializeJson() => serializer.Deserialize(json);

            // Assert
            Assert.Throws<FormatException>(DeserializeJson);
        }

        [Test]
        public void CreateSessionWebViewToUnityEvent_ThrowsForInvalidJsonStructure()
        {
            // Arrange
            const string json = @"{{{{{";
            var serializer = new CreateSessionWebViewEventSerializer();

            // Act
            void DeserializeJson() => serializer.Deserialize(json);

            // Assert
            Assert.Throws<JsonReaderException>(DeserializeJson);
        }
    }
}