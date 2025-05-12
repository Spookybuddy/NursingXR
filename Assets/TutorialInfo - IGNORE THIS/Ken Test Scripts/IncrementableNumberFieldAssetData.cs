using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Initialization Notes
//Always make sure an asset data class is serializable or there will be an error (add (using System;) too)

//This script is not a MonoBehavior, change that to BaseAssetData (also auto-adds (using GIGXR.Platform.Scenarios.GigAssets.Data;), add yourself if it doesn't)
*/
[Serializable]
public class IncrementableNumberFieldAssetData : BaseAssetData
{    
    /* AssetPropertyDefinition Notes
     * Basically variables? seems like it
     * do not need to add default values (even tho it can be done); usually give the values within the scenarios
     * 
     * thats it; this entire script is pretty much just a container for the variables needed for the next script (AssetTypeComponent)
     */
    
    // value to be displayed in the number field
    public AssetPropertyDefinition<int> currentValue;

    // the bounds used to contrain the current value
    public AssetPropertyDefinition<int> minValue;
    public AssetPropertyDefinition<int> maxValue;
}
