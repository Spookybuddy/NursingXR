using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Core.Audio
{
    public class StageScenarioEventsData<T> : ScriptableObject
    {
        public virtual void ExecuteEvent(IEnumerable<T> executeList)
        {
        }
    }
}
