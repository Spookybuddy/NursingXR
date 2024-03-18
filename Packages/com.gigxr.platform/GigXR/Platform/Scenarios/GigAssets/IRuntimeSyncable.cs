using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows for runtime data to be synced over the Network Layer (i.e. Photon), without GMS.
/// </summary>
public interface IRuntimeSyncable
{
    void Sync();
}
