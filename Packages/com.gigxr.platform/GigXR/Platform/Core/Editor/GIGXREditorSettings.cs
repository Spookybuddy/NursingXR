namespace GIGXR.Platform.Core
{
    using UnityEditor;
    using UnityEditor.SettingsManagement;

    public static class GIGXREditorSettings
    {
        static Settings s_SettingsInstance;

        public static Settings Instance
        {
            get
            {
                if (s_SettingsInstance == null)
                    s_SettingsInstance = new Settings("com.GIGXR");

                return s_SettingsInstance;
            }
        }
    }

    static class GIGXREditorSettingsProvider
    {
        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            // The last parameter tells the provider where to search for settings.
            var provider = new UserSettingsProvider("Project/GIGXR Editor Settings",
                GIGXREditorSettings.Instance,
                new[] { typeof(GIGXREditorSettingsProvider).Assembly });

            return provider;
        }
    }

    public static class EditorAuthenticationProfile
    {
        [UserSetting("Authentication Settings", "Test Credentials")]
        public static UserSetting<TestCredentials> TestCredentials = new UserSetting<TestCredentials>(GIGXREditorSettings.Instance,
                                                                                                      nameof(TestCredentials),
                                                                                                      null,
                                                                                                      SettingsScope.User);

        public static TestCredentials GetTestCredentials()
        {
            return TestCredentials.value;
        }
    }
}