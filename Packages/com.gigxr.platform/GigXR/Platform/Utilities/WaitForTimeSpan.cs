using System;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// A coroutine that resumes after the provided <c>TimeSpan</c> has passed.
    /// </summary>
    /// <remarks>
    /// Like <c>WaitForSeconds</c> but accepts a <c>TimeSpan</c>.
    /// </remarks>
    public class WaitForTimeSpan : CustomYieldInstruction
    {
        private readonly DateTime timeToResume;

        public WaitForTimeSpan(TimeSpan timeSpan)
        {
            timeToResume = DateTime.UtcNow.Add(timeSpan);
        }

        public override bool keepWaiting => DateTime.UtcNow < timeToResume;
    }
}