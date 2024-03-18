namespace GIGXR.Platform.Scenarios.Stages.Data
{
    using Newtonsoft.Json;
    using System;
    using UnityEngine;

    /// <summary>
    /// Data class to hold information for a single stage
    /// </summary>
    [Serializable]
    public class Stage
    {
        public string stageId;

        [Header("Stage Title to show the user in Play/Edit Mode. Spaces allowed.")]
        public string stageTitle;

        [JsonConstructor]
        public Stage(string stageId, string stageTitle)
        {
            this.stageId = stageId;
            this.stageTitle = stageTitle;
        }

        public Stage(Guid stageId, string stageTitle) : this(stageId.ToString(), stageTitle)
        {
        }

        [JsonIgnore]
        public Guid StageId
        {
            get => Guid.Parse(stageId);
            set => stageId = value.ToString();
        }
    }
}