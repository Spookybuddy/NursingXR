namespace GIGXR.Platform.Scenarios.EventArgs
{
    using GIGXR.Platform.Scenarios.Data;
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using GIGXR.Platform.Scenarios.Stages.Data;
    using System;
    using System.Collections.Generic;

    public class ScenarioLoadedEventArgs : EventArgs
    {
        public IEnumerable<Stage> Stages { get; }

        public IEnumerable<Asset> Assets { get; }

        public IEnumerable<PathwayData> Pathways { get; }

        public ScenarioLoadedEventArgs(IEnumerable<Stage> stagesLoaded, IEnumerable<Asset> assetsInstantiated, IEnumerable<PathwayData> pathways)
        {
            Stages = stagesLoaded;
            Assets = assetsInstantiated;
            Pathways = pathways;
        }
    }
}