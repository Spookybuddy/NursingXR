using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class IncrementableNumberFieldAssetTypeComponent : BaseAssetTypeComponent<IncrementableNumberFieldAssetData>
{
    [Tooltip("The MRTK Interactable used to press the increment button.")]
    [SerializeField] private Interactable incrementButtonInteractable;

    [Tooltip("The MRTK Interactable used to press the decrememt button.")]
    [SerializeField] private Interactable decrementButtonInteractable;

    [Tooltip("The text to display the current value of the number field.")]
    [SerializeField] private TMP_Text valueDisplayText;

    private IScenarioManager scenarioManager;

    #region Dependencies

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectedScenarioManager)
    {
        scenarioManager = injectedScenarioManager;
    }

    #endregion

    #region BaseAssetTypeComponent overrides

    public override void SetEditorValues()
    {

    }

    protected override void Setup()
    {
        incrementButtonInteractable.OnClick.AddListener(OnIncrementClicked);
        decrementButtonInteractable.OnClick.AddListener(OnDecrementClicked);
    }

    protected override void Teardown()
    {
        incrementButtonInteractable.OnClick.RemoveListener(OnIncrementClicked);
        decrementButtonInteractable.OnClick.RemoveListener(OnDecrementClicked);
    }

    #endregion

    #region Button Listeners

    public void OnIncrementClicked()
    {
        // if the scenario is not playing, do nothing
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        assetData.currentValue.runtimeData.Value += 1;
    }

    public void OnDecrementClicked()
    {
        // if the scenario is not playing, do nothing
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        assetData.currentValue.runtimeData.Value -= 1;
    }

    #endregion

    #region Property Change Handlers

    [RegisterPropertyChange(nameof(IncrementableNumberFieldAssetData.currentValue))]
    private void OnCurrentValueChanged(AssetPropertyChangeEventArgs args)
    {
        int newValue = (int)args.AssetPropertyValue;

        valueDisplayText.text = newValue.ToString();
    }

    [RegisterPropertyChange(nameof(IncrementableNumberFieldAssetData.minValue))]
    private void OnMinValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized) return;

        int newMinValue = (int)args.AssetPropertyValue;

        if (newMinValue > assetData.currentValue.runtimeData.Value)
        {
            assetData.currentValue.runtimeData.UpdateValueLocally(newMinValue);
            valueDisplayText.text = newMinValue.ToString();
        }
    }

    [RegisterPropertyChange(nameof(IncrementableNumberFieldAssetData.maxValue))]
    private void OnMaxValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized) return;

        int newMaxValue = (int)args.AssetPropertyValue;

        if (newMaxValue < assetData.currentValue.runtimeData.Value)
        {
            assetData.currentValue.runtimeData.UpdateValueLocally(newMaxValue);
            valueDisplayText.text = newMaxValue.ToString();
        }
    }

    #endregion

    #region Asset Property Validators

    [RegisterPropertyValidator(nameof(IncrementableNumberFieldAssetData.currentValue))]
    public (object, bool) ValidateCurrentValue(object value)
    {
        if (!IsInitialized) return (value, true);

        int intValue = (int)value;

        if (intValue < assetData.minValue.runtimeData.Value)
        {
            intValue = assetData.minValue.runtimeData.Value;
        }

        if (intValue > assetData.maxValue.runtimeData.Value)
        {
            intValue = assetData.maxValue.runtimeData.Value;
        }

        return (intValue, true);
    }

    #endregion
}