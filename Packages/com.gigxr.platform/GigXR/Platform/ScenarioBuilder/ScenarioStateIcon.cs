using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple visualizer for scenario state in PSBuilder
/// </summary>
public class ScenarioStateIcon : MonoBehaviour
{
    [SerializeField]
    private Text debugScenarioStateText;
    
    [SerializeField]
    [Header("NOTE: Edit Mode should not be used in PSB other than troubleshooting controls.")]
    private Sprite EditingIcon;
    
    [SerializeField]
    private Sprite PlayingIcon;

    [SerializeField]
    private Sprite PausedIcon;

    [SerializeField]
    private Sprite StoppedIcon;

    private Image imageComponent;

#region InjectDependencies

    private IScenarioManager ScenarioManager;

    [InjectDependencies]
    public void Construct(IScenarioManager scenarioManager)
    {
        ScenarioManager = scenarioManager;
    }

#endregion

    private void Start()
    {
        imageComponent = gameObject.GetComponent<Image>();

        ScenarioManager.ScenarioPlaying += (sender, args) =>
        {
            debugScenarioStateText.text = "Playing";
            imageComponent.sprite       = PlayingIcon;
        };

        ScenarioManager.ScenarioPaused += (sender, args) =>
        {
            debugScenarioStateText.text = "Paused";
            imageComponent.sprite       = PausedIcon;
        };

        ScenarioManager.ScenarioStopped += (sender, args) =>
        {
            debugScenarioStateText.text = "Stopped";
            imageComponent.sprite       = StoppedIcon;
        };

        ScenarioManager.ScenarioReset += (sender, args) =>
        {
            debugScenarioStateText.text = "Loaded";
            imageComponent.sprite = null;
        };
    }

    [Serializable]
    public enum ScenarioStates
    {
        Stopped,
        Editing,
        Paused,
        Playing
    }

    public void SetScenarioState(string targetState)
    {
        var scenarioState = (ScenarioStates)Enum.Parse(typeof(ScenarioStates), targetState);

        switch (scenarioState)
        {
            case ScenarioStates.Stopped:
                ScenarioManager.StopScenarioAsync();
                break;
            case ScenarioStates.Paused:
                ScenarioManager.PauseScenarioAsync();
                break;
            case ScenarioStates.Playing:
                ScenarioManager.PlayScenarioAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}