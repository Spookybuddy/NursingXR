using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class SprayAssetTypeComponent : BaseAssetTypeComponent<SprayAssetData>
{
    [Tooltip("The MRTK Interactible used to press the spray button.")]
    [SerializeField] private Interactable sprayButtonInteractible;

    [Tooltip("The particle system for the spray bottle.")]
    [SerializeField] private ParticleSystem sprayParticles;

    private IScenarioManager scenarioManager;

    #region Dependencies

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectedScenarioManager)
    {
        scenarioManager = injectedScenarioManager;
    }

    #endregion

    #region BaseAssetTypeComponent overrides

    //Sets default values on any asset that gets made from the prefab with this as a component.
    public override void SetEditorValues()
    {

    }

    //GigXR equivalent to Unity's Awake (called when an enabled script instance is being loaded)
    protected override void Setup()
    {
        sprayButtonInteractible.OnClick.AddListener(OnSprayClicked);
    }

    //GigXR equivalent to Unity's OnDestroy (called when this component is destroyed)
    protected override void Teardown()
    {
        sprayButtonInteractible.OnClick.RemoveListener(OnSprayClicked);
    }

    #endregion

    #region Button Listeners

    private void OnSprayClicked()
    {
        if (scenarioManager.ScenarioStatus != GIGXR.Platform.Scenarios.Data.ScenarioStatus.Playing)
        {
            return;
        }
        Debug.Log("Attempting To Spray");

        assetData.activateSpray.runtimeData.Value = !assetData.activateSpray.runtimeData.Value;
    }

    #endregion

    #region Property Change Handlers


    //[RegisterPropertyChange] makes the function a listener for when a certain specified property changes
    [RegisterPropertyChange(nameof(SprayAssetData.activateSpray))]
    private void OnCurrentValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (sprayParticles.isStopped)
        {
            sprayParticles.Play();
        }
    }

    #endregion
}
