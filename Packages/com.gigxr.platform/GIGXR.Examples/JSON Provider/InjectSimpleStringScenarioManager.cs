using GIGXR.Platform.Core;
using GIGXR.Platform.Scenarios;

// Class acts as a Factory for the custom Scenario Generator
// Place this class alongside the GIGXRCore/HMD Composition Root
public class InjectSimpleStringScenarioManager : CoreInjectorComponent<IScenarioManager>
{
    public override IScenarioManager GetSingleton()
    {
        return new SimpleStringScenarioManager();
    }
}
