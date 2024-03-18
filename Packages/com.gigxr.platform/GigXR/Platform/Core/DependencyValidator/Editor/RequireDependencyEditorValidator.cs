using UnityEditor.Build;
using System;

namespace GIGXR.Platform.Core.DependencyValidator
{
    public class RequireDependencyEditorValidator
    {
        // Split up to possibly configure addressables vs all build prefabs etc slower options
        public static void ValidateDependenciesForBuild()
        {
            RequireDependencyAddressables.ValidateAddressables();
        }

        // This wraps exceptions from ValidateAddressables into BuildFailedException
        // That's SUPPOSED to cancel an ongoing player build, but in some versions of Unity this does not
        public static void ValidateAddressablesForBuild()
        {
            try
            {
                RequireDependencyAddressables.ValidateAddressables();
            }
            catch (Exception e)
            {
                throw new BuildFailedException($"[RequireDependencyBuildProcessor] {e.Message}");
            }
        }

        // Similar to ValidateAddressablesForBuild() but boolean vs Exceptions for Tests
        public static bool ValidateAddressablesForTests()
        {
            try
            {
                RequireDependencyAddressables.ValidateAddressables();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}