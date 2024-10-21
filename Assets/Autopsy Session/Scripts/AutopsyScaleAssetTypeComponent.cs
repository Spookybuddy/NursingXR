using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class AutopsyScaleAssetTypeComponent : BaseAssetTypeComponent<AutopsyScaleAssetData>
{
    [Tooltip("The text to display the current weight of all objects on the scale.")]
    [SerializeField] private TMP_Text weightDisplayText;

    private List<AutopsyBodyPartAssetTypeComponent> objectsOnScale;

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
        objectsOnScale = new List<AutopsyBodyPartAssetTypeComponent>();
    }

    protected override void Teardown()
    {
        
    }

    #endregion

    #region Getters and Setters

    public bool IsObjectOnScale(AutopsyBodyPartAssetTypeComponent obj)
    {
        return objectsOnScale.Contains(obj);
    }

    public void AddObjectToScale(AutopsyBodyPartAssetTypeComponent obj)
    {
        if (!objectsOnScale.Contains(obj))
        {
            Debug.Log("Object Added In Scale Behavior");
            objectsOnScale.Add(obj);
            float weightSum = 0;
            foreach (AutopsyBodyPartAssetTypeComponent bodyPart in objectsOnScale)
            {
                weightSum += bodyPart.GetWeight();
            }
            assetData.weightOnScale.runtimeData.Value = weightSum;
        }
    }

    public void RemoveObjectFromScale(AutopsyBodyPartAssetTypeComponent obj)
    {
        if (objectsOnScale.Contains(obj))
        {
            Debug.Log("Removing Object");
            objectsOnScale.Remove(obj);
            float weightSum = 0;
            foreach (AutopsyBodyPartAssetTypeComponent bodyPart in objectsOnScale)
            {
                weightSum += bodyPart.GetWeight();
            }
            assetData.weightOnScale.runtimeData.Value = weightSum;
        }
    }

    #endregion

    #region Property Change Handlers

    [RegisterPropertyChange(nameof(AutopsyScaleAssetData.weightOnScale))]
    private void OnScaleWeightChanged(AssetPropertyChangeEventArgs args)
    {
        float newValue = (float)args.AssetPropertyValue;
        Debug.Log("Detected Value Change");
        if (newValue > .01f)
        {
            weightDisplayText.text = newValue.ToString() + " kg";
        }
        else
        {
            weightDisplayText.text = "Place Object";
        }
    }

    #endregion
}
