namespace GIGXR.Platform.Sessions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Holds all the <see cref="ISessionCapability"/> that are granted to the local user.
    /// </summary>
    public class SessionUser
    {
        private Dictionary<Type, ISessionCapability> capabilities = new Dictionary<Type, ISessionCapability>();

        public void AddSessionCapability(ISessionCapability sessionCapability)
        {
            if(!capabilities.ContainsKey(sessionCapability.GetType()))
            {
                sessionCapability.Activate();

                capabilities.Add(sessionCapability.GetType(), sessionCapability);
            }
        }

        public void RemoveSessionCapability(Type capabilityType)
        {
            if (capabilities.ContainsKey(capabilityType))
            {
                capabilities[capabilityType].Deactivate();

                capabilities.Remove(capabilityType);
            }
        }

        public void RemoveAll()
        {
            foreach(var capability in capabilities.Values.ToArray())
            {
                capability.Deactivate();
            }

            capabilities.Clear();
        }
    }
}