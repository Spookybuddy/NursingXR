using System;
using GIGXR.Platform.Utilities;
using NUnit.Framework;

namespace GIGXR.Test.Editor.Platform.Utilities
{
    public class UriExtensionsTests
    {
        [Test]
        public void AppendPath_AppendsPathToRootDomain()
        {
            // Arrange
            var baseUri = new Uri("https://example.com?test=query");

            // Act
            var uri = baseUri.AppendPath("path/example");

            // Assert
            Assert.AreEqual("https://example.com/path/example?test=query", uri.ToString());
        }

        [Test]
        public void AppendPath_AppendsPathToSubdirectory()
        {
            // Arrange
            var baseUri = new Uri("https://example.com/api/v1?test=query");

            // Act
            var uri = baseUri.AppendPath("path/example");

            // Assert
            Assert.AreEqual("https://example.com/api/v1/path/example?test=query", uri.ToString());
        }

        [Test]
        public void AppendQueryString_AppendsWithoutExistingQueryString()
        {
            // Arrange
            var baseUri = new Uri("https://example.com/api/v1");

            // Act
            var uri = baseUri.AppendQueryString("test=query&foo=bar");

            // Assert
            Assert.AreEqual("https://example.com/api/v1?test=query&foo=bar", uri.ToString());
        }

        [Test]
        public void AppendQueryString_AppendsToExistingQueryString()
        {
            // Arrange
            var baseUri = new Uri("https://example.com/api/v1?existing=query");

            // Act
            var uri = baseUri.AppendQueryString("test=query&foo=bar");

            // Assert
            Assert.AreEqual("https://example.com/api/v1?existing=query&test=query&foo=bar", uri.ToString());
        }

        [Test]
        public void AppendQueryString_IgnoresLeadingSymbol()
        {
            // Arrange
            var baseUri = new Uri("https://example.com/api/v1?existing=query");
            const string expectedResult = "https://example.com/api/v1?existing=query&test=query&foo=bar";

            // Act
            var uri0 = baseUri.AppendQueryString("&test=query&foo=bar");
            var uri1 = baseUri.AppendQueryString("?test=query&foo=bar");

            // Assert
            Assert.AreEqual(expectedResult, uri0.ToString());
            Assert.AreEqual(expectedResult, uri1.ToString());
        }
    }
}