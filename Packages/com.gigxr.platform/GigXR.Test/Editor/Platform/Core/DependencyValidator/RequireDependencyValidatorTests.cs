using GIGXR.Platform.Core.DependencyValidator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GIGXR.Test.Editor.Platform.Core.DependencyValidator
{
    public class RequireDependencyValidatorTests
    {
        [Test]
        public void RequireDependencyValidator_ReturnsTrueWhenAllDependenciesHaveValues()
        {
            // Arrange
            var gameObject = new GameObject();
            var component = gameObject.AddComponent<TestComponent>();
            component.SetTestDependency(new object());
            var validator = new RequireDependencyValidator();

            // Act
            var result = validator.ValidateRequiredDependenciesInScene();
            Object.DestroyImmediate(gameObject);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void RequireDependencyValidator_ReturnsFalseWhenDependenciesWereNotDefined()
        {
            // Arrange
            var gameObject = new GameObject();
            gameObject.AddComponent<TestComponent>();
            LogAssert.ignoreFailingMessages = true;
            var validator = new RequireDependencyValidator();

            // Act
            var result = validator.ValidateRequiredDependenciesInScene();
            Object.DestroyImmediate(gameObject);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RequireDependencyValidator_ReturnsFalseWhenDependenciesAreNull()
        {
            // Arrange
            var gameObject = new GameObject();
            var component = gameObject.AddComponent<TestComponent>();
            component.SetTestDependency(null);
            LogAssert.ignoreFailingMessages = true;
            var validator = new RequireDependencyValidator();

            // Act
            var result = validator.ValidateRequiredDependenciesInScene();
            Object.DestroyImmediate(gameObject);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RequireDependencyValidator_ReturnsFalseForMissingParentDependency()
        {
            // Arrange
            var gameObject = new GameObject();
            var component = gameObject.AddComponent<ChildTestComponent>();
            component.SetTestDependency(null);
            component.SetChildDependency(new object());
            LogAssert.ignoreFailingMessages = true;
            var validator = new RequireDependencyValidator();
            
            // Act
            var result = validator.ValidateRequiredDependenciesInScene();
            Object.DestroyImmediate(gameObject);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RequireDependencyValidator_ReturnsFalseForMissingChildDependency()
        {
            // Arrange
            var gameObject = new GameObject();
            var component = gameObject.AddComponent<ChildTestComponent>();
            component.SetTestDependency(new object());
            component.SetChildDependency(null);
            LogAssert.ignoreFailingMessages = true;
            var validator = new RequireDependencyValidator();
            
            // Act
            var result = validator.ValidateRequiredDependenciesInScene();
            Object.DestroyImmediate(gameObject);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RequireDependencyValidator_ReturnsFalseForBothMissingChildAndParentDependency()
        {
            // Arrange
            var gameObject = new GameObject();
            var component = gameObject.AddComponent<ChildTestComponent>();
            component.SetTestDependency(null);
            component.SetChildDependency(null);
            LogAssert.ignoreFailingMessages = true;
            var validator = new RequireDependencyValidator();
            
            // Act
            var result = validator.ValidateRequiredDependenciesInScene();
            Object.DestroyImmediate(gameObject);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RequireDependencyValidator_ReturnsTrueForEntireInheritanceChain()
        {
            // Arrange
            var gameObject = new GameObject();
            var component = gameObject.AddComponent<ChildTestComponent>();
            component.SetTestDependency(new object());
            component.SetChildDependency(new object());
            LogAssert.ignoreFailingMessages = true;
            var validator = new RequireDependencyValidator();
            
            // Act
            var result = validator.ValidateRequiredDependenciesInScene();
            Object.DestroyImmediate(gameObject);

            // Assert
            Assert.IsTrue(result);
        }

        private class TestComponent : MonoBehaviour
        {
            [SerializeField, RequireDependency] private object testAlreadySetDependency = new object();

            [SerializeField, RequireDependency] private object testDependency;

            public void SetTestDependency(object value) => testDependency = value;
        }

        private class ChildTestComponent : TestComponent
        {
            [SerializeField, RequireDependency] private object childDependency;
            
            public void SetChildDependency(object value) => childDependency = value;
        }
    }
}