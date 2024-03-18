namespace GIGXR.Platform.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Helpers for accessing assemblies.
    /// </summary>
    /// <remarks>
    /// There are mild incompatibilities between some of our dependencies. When <c>com.unity.code-analysis</c> was
    /// added, it had its own version of <c>System.Runtime.Loader</c>, but there was already another version in the
    /// project. This caused a <c>ReflectionTypeLoadException</c> to occur when trying to enumerate over all of the
    /// Types in the currently loaded assemblies.
    ///
    /// This helper was added to be able to access the Types that are loaded successfully, ignoring any types that have
    /// runtime issues.
    /// </remarks>
    public static class AssemblyHelper
    {
        /// <summary>
        /// Get all Types in the currently loaded Assemblies. Ignores Types that cannot be loaded due to runtime issues.
        /// </summary>
        /// <returns>A list of Types that are currently loaded in memory.</returns>
        public static IList<Type> GetTypesInAllLoadedAssembliesSafely()
        {
            // Get an IEnumerable of all Types currently loaded in-memory. This should exclude assemblies that are not
            // loaded at the time this is called. This uses deferred execution so any Exceptions will not be raised until
            // this is enumerated over.
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes());

            var result = new List<Type>();

            // Enumerate manually checking for Exceptions loading each Type.
            using var enumerator = types.GetEnumerator();
            var shouldContinue = true;
            do
            {
                try
                {
                    shouldContinue = enumerator.MoveNext();
                    var type = enumerator.Current;
                    result.Add(type);
                }
                catch (ReflectionTypeLoadException exception)
                {
                    // Ignore and move on. Log the LoaderExceptions to see which Type could not be loaded.
                    // foreach (var inner in exception.LoaderExceptions)
                    // {
                    //     Debug.LogException(inner);
                    // }
                }
            } while (shouldContinue);

            return result;
        }
    }
}