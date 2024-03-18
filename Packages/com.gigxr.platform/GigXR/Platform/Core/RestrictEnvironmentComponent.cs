using GIGXR.Platform;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Core.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RestrictEnvironmentComponent : MonoBehaviour
{
    private AuthenticationProfile profile;

    public bool isQAOnly = false;

    public UnityEvent<bool> SwitchState;

    [InjectDependencies]
    public void Construct(ProfileManager profileManager)
    {
        profile = profileManager.authenticationProfile;
    }

    private void Update()
    {
        if(profile != null)
        {
            bool canActivate = profile.TargetEnvironmentalDetails.IsQAEnvironment && isQAOnly;

            // We are in a QA environment and this component is allowed
            if(gameObject.activeInHierarchy != canActivate)
            {
                SwitchState?.Invoke(canActivate);
            }
        }
    }
}