using GIGXR.Platform.Scenarios.GigAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Scenarios
{
    public class RuntimeStageInput
    {
        public string stageId;
        public bool useShared;
        public bool resetValueOnStageChange;
        public object localValue;
        public int stageIndex;

        public RuntimeStageInput(string stage, bool shared, bool reset, object value, int index)
        {
            stageId = stage;
            useShared = shared;
            resetValueOnStageChange = reset;
            localValue = value;
            stageIndex = index;
        }
    }

    public class StageInput
    {
        public IAssetPropertyDefinition assetPropertyDefinition;
        public int specificStageIndex;
        public RuntimeStageInput runtimeData;

        public StageInput(IAssetPropertyDefinition property, int index, RuntimeStageInput runtime = null)
        {
            assetPropertyDefinition = property;
            specificStageIndex = index;
            runtimeData = runtime;
        }
    }
}