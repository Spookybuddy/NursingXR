using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Sessions
{
    public interface ISessionCapability
    {
        void Activate();
        void Deactivate();
    }
}