using System;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// Wait until the provided predicate is <c>false</c>, or until the provided <c>TimeSpan</c> has passed.
    /// </summary>
    /// <remarks>
    /// Like <c>WaitWhile()</c> and <c>WaitForTimeSpan()</c> combined.
    /// </remarks>
    public class WaitWhileOrTimeout : CustomYieldInstruction
    {
        private readonly Func<bool> predicate;
        private readonly DateTime timeToResume;

        public WaitWhileOrTimeout(Func<bool> predicate, TimeSpan timeout)
        {
            this.predicate = predicate;
            timeToResume = DateTime.UtcNow.Add(timeout);
        }

        public override bool keepWaiting
        {
            get
            {
                if (!predicate())
                    return false;

                return DateTime.UtcNow < timeToResume;
            }
        }
    }
}