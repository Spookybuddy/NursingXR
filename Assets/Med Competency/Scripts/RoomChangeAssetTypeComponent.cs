using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using Microsoft.MixedReality.Toolkit.UI;
using GIGXR.Platform.Scenarios.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using System;

public class RoomChangeAssetTypeComponent : BaseAssetTypeComponent<RoomChangeAssetData>
{
    //Buttons
    [Tooltip("The MRTK Interactible used to switch from one 'room' to another")]
    [SerializeField] private Interactable nextRoomButton;

    [Tooltip("The text to disply which room is active/ the user is in")]
    [SerializeField] private TMP_Text roomLabelText;

    //Managers
    private IScenarioManager scenarioManager;

    [SerializeField] private IsEnabledAssetTypeComponent hospitalBedEnabledComponent;
    [SerializeField] private IsEnabledAssetTypeComponent patientBoardEnabledComponent;
    [SerializeField] private IsEnabledAssetTypeComponent medCabinetEnabledComponent;
    [SerializeField] private IsEnabledAssetTypeComponent tempMedsEnabledComponent;


    #region Dependencies

    [InjectDependencies]
    public void InjectDependencies (IScenarioManager injectScenarioManager)
    {
        scenarioManager = injectScenarioManager;
    }

    #endregion

    #region BaseAssetTypeComponent overrides
    public override void SetEditorValues()
    {

    }

    protected override void Setup()
    {
        nextRoomButton.OnClick.AddListener(OnNextRoomClick);
    }

    protected override void Teardown()
    {
        nextRoomButton.OnClick.RemoveListener(OnNextRoomClick);
    }

    #endregion

    #region Button Listeners

    private void OnNextRoomClick()
    {
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        hospitalBedEnabledComponent = GameObject.Find("hospital-bed (HospitalBed(Clone))").GetComponent<IsEnabledAssetTypeComponent>();
        patientBoardEnabledComponent = GameObject.Find("patientBoard (PatientBoard(Clone))").GetComponent<IsEnabledAssetTypeComponent>();
        medCabinetEnabledComponent = GameObject.Find("med-cabinet (Med Cabinet(Clone))").GetComponent<IsEnabledAssetTypeComponent>();
        tempMedsEnabledComponent = GameObject.Find("temp-med (TempMeds(Clone))").GetComponent<IsEnabledAssetTypeComponent>();

        assetData.inPatientRoom.runtimeData.Value = !assetData.inPatientRoom.runtimeData.Value;
    }

    #endregion

    #region Property Change Handlers

    [RegisterPropertyChange(nameof(RoomChangeAssetData.inPatientRoom))]
    private void OnRoomChanged(AssetPropertyChangeEventArgs args)
    {
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        bool roomValue = (bool)args.AssetPropertyValue;

        if (roomValue == true)
        {
            roomLabelText.text = "Patient Room";

            hospitalBedEnabledComponent.AssetData.isEnabled.runtimeData.Value = true;
            patientBoardEnabledComponent.AssetData.isEnabled.runtimeData.Value = true;
            
            medCabinetEnabledComponent.AssetData.isEnabled.runtimeData.Value = false;
        }
        else if (roomValue == false)
        {
            roomLabelText.text = "Med Room";

            hospitalBedEnabledComponent.AssetData.isEnabled.runtimeData.Value = false;
            patientBoardEnabledComponent.AssetData.isEnabled.runtimeData.Value = false;

            medCabinetEnabledComponent.AssetData.isEnabled.runtimeData.Value = true;
            tempMedsEnabledComponent.AssetData.isEnabled.runtimeData.Value = true;
        }

    }

    #endregion


}
