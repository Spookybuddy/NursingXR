using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using GIGXR.Platform.Scenarios.Data;
using TMPro;
using UnityEngine;

/* Initialization Notes
 * 
 * This script changes from a MonoBehavior to BaseAssetTypeComponent
 * While it technically is a MonoBehavior, it is not going to be explicitly labeled as as such because it extends the BaseAssetTypeComponent (basically is a MonoBehavior)
 * Auto adds (using GIGXR.Platform.Scenarios.GigAssets;)
 * 
 * Inside the <> is the BaseAssetData script that goes with it (the one we just made)
 * 
 * BaseAssetTypeComponent class will not work unless the abstract methods (SetEditorValues, Setup, and TearDown) are within it. (in the overrides region)
 * Going into "Show Potential Fixes" and then "Implement Abstract Class" will auto-add them
 */
public class IncrementableNumberFieldAssetTypeComponent : BaseAssetTypeComponent<IncrementableNumberFieldAssetData>
{
    [Tooltip("The MRTK Interactable used to press the increment button.")]
    [SerializeField] private Interactable incerementButtonInteractible;

    [Tooltip("The MRTK Interactable used to press the decrement button.")]
    [SerializeField] private Interactable decrementButtonInteractible;

    [Tooltip("The text which will display the number field's current value.")]
    [SerializeField] private TMP_Text valueDisplayText;

    private IScenarioManager scenarioManager;

    #region Dependencies
    /* Dependency Notes
     * 
     * In order to access info about the scenarios (their status), access to the Scenario Manager is needed
     * To get access to the scenario manager, it needs to be "injected" (added to the script)
     * 
     * When adding the "IScenarioManager" it auto-imports the "using GIGXR.Platform.Scenarios;"
     * 
     * Need to add "[InjectDependencies]" before the method and "using GIGXR.Platform.Core.DependencyInjection;" to make that work. error otherwise will show
     * "Object reference not set to an instance of an object" when trying to click buttons
     * 
     * NOTE: Injection occurs before the Setup method
     */

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager InjectedScenarioManager)
    {
        scenarioManager = InjectedScenarioManager;
    }
    #endregion

    #region BaseAssetTypeComponent overrides
    // Sets default values on new prefabs when you make assets
    public override void SetEditorValues()
    {
        
    }

    // Equivalent to Unity's "OnAwake", calls when the asset is initilaized to set it up 
    protected override void Setup()
    {
        // Adds listeners to the buttons on startup so that the when the button is clicked it calls the method in the parentheses
        incerementButtonInteractible.OnClick.AddListener(OnIncrementClicked);
        decrementButtonInteractible.OnClick.AddListener(OnDecrementClicked);
    }

    // Equivalent to Unity's "OnDestroy", calls when the asset is removed when the scenario is closed
    protected override void Teardown()
    {
        // When the asset is destroyed, remove the listenesrs from the buttons
        //This is not really necessary since the butttons will be destroyed as well, but its just for the example
        incerementButtonInteractible.OnClick.RemoveListener(OnIncrementClicked);
        decrementButtonInteractible.OnClick.RemoveListener(OnDecrementClicked);
    }
    #endregion

    #region ButtonListeners
    private void OnIncrementClicked()
    {
        /* Notes
         * "GIGXR.Platform.Scenarios.Data" is needed to get the Scenario Status. That can be imported as a "using _" 
         * 
         * To change the displayed variables, do not enter the value in the TMP variable. This is because this will only affect the user that made the change and would not update other users.
         * Instead, change the variable in the AssetDataClass (currentValue), that will spread through the system.
         */

        //If the scenario is not playing, then nothing happens
        if (scenarioManager.ScenarioStatus != GIGXR.Platform.Scenarios.Data.ScenarioStatus.Playing) return;

        assetData.currentValue.runtimeData.Value++;
    }

    private void OnDecrementClicked()
    {
        //If the scenario is not playing, then nothing happens
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        assetData.currentValue.runtimeData.Value--;
    }
    #endregion

    #region Property Change Handlers
    /* Property Change Handler Notes
     * 
     * These methods do things if the property (variable) value is changed.
     * 
     * Adding the "AssetPropertyChangeEventArgs" auto-added the "using GIGXR.Platform.Scenarios.GigAssets.EventArgs;" up top
     * 
     * RegisteringPropertyChange does ____
     * "nameof" makes it so if the name of the variable on the other script was to ever change, it would still be able to recognize it and the script wouldn't break
     */

    [RegisterPropertyChange(nameof(IncrementableNumberFieldAssetData.currentValue))]
    private void OnValueChange(AssetPropertyChangeEventArgs args)
    {
        //The argument is a generic variable so this line assigns it to its proper variable type (int in this case)
        int newValue = (int)args.AssetPropertyValue;

        valueDisplayText.text = newValue.ToString();
    }

    [RegisterPropertyChange(nameof(IncrementableNumberFieldAssetData.minValue))]
    //In the case that the min/max value is changed, checks to see if the current value is less/more than the new version of the min/max value and if so, brings it back into the bounds
    private void onMinValueChanged(AssetPropertyChangeEventArgs args)
    {
        //Prevents this method if the value hasn't been initialized yet

        if (!IsInitialized) return;
        
        int newMinValue = (int)args.AssetPropertyValue;

        if (newMinValue > assetData.currentValue.runtimeData.Value)
        {
            //This line updates the value locally so the whole process does not need to be done for all of the ones in the network
            assetData.currentValue.runtimeData.UpdateValueLocally(newMinValue);
            valueDisplayText.text = newMinValue.ToString();
        }
    }
    [RegisterPropertyChange(nameof(IncrementableNumberFieldAssetData.maxValue))]
    private void onMaxValueChanged(AssetPropertyChangeEventArgs args)
    {
        //Prevents this method if the value hasn't been initialized yet

        if (!IsInitialized) return;

        int newMaxValue = (int)args.AssetPropertyValue;

        if (newMaxValue < assetData.currentValue.runtimeData.Value)
        {
            //This line updates the value locally so the whole process does not need to be done for all of the ones in the network
            assetData.currentValue.runtimeData.UpdateValueLocally(newMaxValue);
            valueDisplayText.text = newMaxValue.ToString();
        }
    }
    #endregion

    #region Asset Property Validators
    /* Validator Notes
     * 
     * Validators take in the value that the property is trying to be asigned and valiate it and then either change it if its valid or reject it and not change anything 
     */

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
