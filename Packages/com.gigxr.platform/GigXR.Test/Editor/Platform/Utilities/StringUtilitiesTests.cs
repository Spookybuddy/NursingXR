using GIGXR.Platform.Utilities;
using NUnit.Framework;

namespace GIGXR.Test.Editor.Platform.Utilities
{
    public class StringUtilitiesTests
    {
        [Test]
        public void AddTrailingSlashIfMissing_ReturnsSlashForEmptyString()
        {
            // Arrange
            var input = "";

            // Act
            var result = StringUtilities.AddTrailingSlashIfMissing(input);

            // Assert
            Assert.AreEqual("/", result);
        }

        [Test]
        public void AddTrailingSlashIfMissing_AddsSlashWhenNeeded()
        {
            // Arrange
            var input = "/test/input";

            // Act
            var result = StringUtilities.AddTrailingSlashIfMissing(input);

            // Assert
            Assert.AreEqual("/test/input/", result);
        }

        [Test]
        public void AddTrailingSlashIfMissing_DoesNotModifyStringWhenSlashPresent()
        {
            // Arrange
            var input = "/test/input/";

            // Act
            var result = StringUtilities.AddTrailingSlashIfMissing(input);

            // Assert
            Assert.AreEqual("/test/input/", result);
        }

        [Test]
        public void RemoveLeadingSlashIfPresent_ReturnsEmptyStringForEmptyString()
        {
            // Arrange
            var input = "";

            // Act
            var result = StringUtilities.RemoveLeadingSlashIfPresent(input);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void RemoveLeadingSlashIfPresent_RemovesLeadingSlashWhenNeeded()
        {
            // Arrange
            var input = "/test/input/";

            // Act
            var result = StringUtilities.RemoveLeadingSlashIfPresent(input);

            // Assert
            Assert.AreEqual("test/input/", result);
        }

        [Test]
        public void RemoveLeadingSlashIfPresent_DoesNotModifyStringWhenSlashMissing()
        {
            // Arrange
            var input = "test/input/";

            // Act
            var result = StringUtilities.RemoveLeadingSlashIfPresent(input);

            // Assert
            Assert.AreEqual("test/input/", result);
        }
    }
}