using GIGXR.Platform.ScenarioBuilder.Data;
using GIGXR.Platform.Scenarios.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

// Plain C# with the Serializable attribute makes this ready to be used as a data container
[Serializable]
public class SimpleStringClass : IScenarioData
{
    public string name;
}

// HmdJsonProvider makes this a ScriptableObject
[CreateAssetMenu(fileName = "New Simple String Provider", menuName = "GIGXR/Examples/New Simple String Provider")]
public class SimpleStringProvider : HmdJsonProvider
{
    public string inputName;

    public override string GetJson()
    {
        return JsonConvert.SerializeObject(GetSimpleString(), Formatting.Indented);
    }

    public override JObject GetDictionary()
    {
        return JObject.FromObject(GetSimpleString());
    }

    private SimpleStringClass GetSimpleString()
    {
        return new SimpleStringClass()
        {
            name = inputName
        };
    }
}
