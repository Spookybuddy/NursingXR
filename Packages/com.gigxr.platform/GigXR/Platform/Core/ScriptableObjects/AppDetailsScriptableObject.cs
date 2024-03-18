using UnityEngine;
using System;
using GIGXR.Platform.Utilities.SerializableDictionary.Example.Example;

namespace GIGXR.Platform.Core
{
    [Serializable]
    public enum Levels
    {
        Major,
        Minor,
        Patch,
        Revision
    }

    /// <summary>
    /// MAJOR version when you make incompatible API changes,
    /// MINOR version when you add functionality in a backwards compatible manner, and
    /// PATCH version when you make backwards compatible bug fixes.
    /// 
    /// Additional labels for pre-release and build metadata are available as extensions to the MAJOR.MINOR.PATCH format.
    /// </summary>
    [HelpURL("https://semver.org/")]
    [CreateAssetMenu(fileName = "App Details", menuName = "GIGXR/ScriptableObjects/App Management/New App Details")]
    public class AppDetailsScriptableObject : ScriptableObject
    {
        public string appName;

        public Sprite AppIcon;

        [SerializeField]
        private GigVersion AppVersion = new GigVersion();

        public string VersionString
            => $"{AppVersion[Levels.Major]}.{AppVersion[Levels.Minor]}.{AppVersion[Levels.Patch]}.{AppVersion[Levels.Revision]}";

        public bool IsVersionCompatible(string stringVersion)
        {
            var versionInput = CreateLevelDictionaryFromStringVersion(stringVersion);

            if (versionInput != null && AppVersion != null)
            {
                // For compatibility, major and minor versions must be exact
                // At the moment, we do not guarantee backwards compatibility 
                // We do not care about revisions here
                return (AppVersion[Levels.Major] == versionInput[Levels.Major] &&
                        AppVersion[Levels.Minor] == versionInput[Levels.Minor] &&
                        AppVersion[Levels.Patch] >= versionInput[Levels.Patch]);
            }
            else
            {
                return false;
            }
        }

        public void SetAppVersionUsingString(string newVersion)
        {
            AppVersion = CreateLevelDictionaryFromStringVersion(newVersion);
        }

        private GigVersion CreateLevelDictionaryFromStringVersion(string newVersion)
        {
            if (string.IsNullOrEmpty(newVersion))
                return null;

            var splitString = newVersion.Split('.');

            var levelDictionary = new GigVersion();

            for (int i = 0; i < 4; i++)
            {
                levelDictionary[(Levels)i] = Convert.ToInt32(splitString[i]);
            }

            return levelDictionary;
        }

        public void BumpAppVersion(Levels level)
        {
            AppVersion[level] += 1;
        }

        public Version AppVersionToSystemVersion()
        {
            return new Version
                (
                    AppVersion[Levels.Major],
                    AppVersion[Levels.Minor],
                    AppVersion[Levels.Patch],
                    AppVersion[Levels.Revision]
                );
        }
    }

    [Serializable]
    public class GigVersion : GenericEnumIntDictionary<Levels>
    {
        public GigVersion() 
        {
            Add(Levels.Major, 0);
            Add(Levels.Minor, 0);
            Add(Levels.Patch, 0);
            Add(Levels.Revision, 0);
        }
    }
}