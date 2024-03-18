using GIGXR.Platform.AppEvents;
using GIGXR.Platform.AppEvents.Events.Session;
using System;

namespace GIGXR.Platform.Sessions
{
    /// <summary>
    /// The set of actions and responsibilities of the session creator user. The session creator is 
    /// the user who started this session originally and who owns the session in GMS.
    /// </summary>
    public class CreatorCapabilities : ISessionCapability, IDisposable
    {
        #region Dependencies

        #endregion

        public CreatorCapabilities()
        {
        }

        #region ISessionCapabilityImplementation

        public void Activate()
        {
            SubscribeToEvents();
        }

        public void Deactivate()
        {
            UnsubscribeToEvents();
        }

        #endregion

        private void SubscribeToEvents()
        {
        }

        private void UnsubscribeToEvents()
        {
        }


        #region IDisposable Implementation

        public void Dispose()
        {
            UnsubscribeToEvents();
        }

        #endregion
    }
}