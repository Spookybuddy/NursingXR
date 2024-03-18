using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace GIGXR.Platform.Core.DependencyValidator
{
    class RequireDependencyBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        // When a Build Player sequence is started this will ideally run beforehand and error out the process if problems exist
        public void OnPreprocessBuild(BuildReport report)
        {
            RequireDependencyEditorValidator.ValidateDependenciesForBuild();
        }
    }
}