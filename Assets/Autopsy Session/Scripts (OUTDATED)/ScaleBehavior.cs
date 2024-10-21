//--------------------------------------------------------------------------------------
// ScaleBehavior
//
// Keeps track of which InteractibleObjects are on the scale and updates the displayed
// weight appropriately.
//--------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBehavior : MonoBehaviour
{
    
    [SerializeField]
    private TMPro.TextMeshPro scaleText;
    private CameraControl camControl;
    private List<InteractibleObject> objectsOnScale, objectsInGravityZone;

    // Start is called before the first frame update
    void Start()
    {
        camControl = GameObject.Find("Main Camera").GetComponent<CameraControl>();
        objectsOnScale = new List<InteractibleObject>();
        objectsInGravityZone = new List<InteractibleObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //--------------------------------------------------------------------------------------
    // Getters and Setters
    //--------------------------------------------------------------------------------------

    public bool IsObjectOnScale(InteractibleObject obj)
    {
        return objectsOnScale.Contains(obj);
    }

    public void AddObjectToScale(InteractibleObject obj)
    {
        if (!objectsOnScale.Contains(obj))
        {
            Debug.Log("Object Added In Scale Behavior");
            objectsOnScale.Add(obj);
            UpdateScaleText();
        }
    }

    public void RemoveObjectFromScale(InteractibleObject obj)
    {
        if (objectsOnScale.Contains(obj))
        {
            Debug.Log("Removing Object");
            objectsOnScale.Remove(obj);
            UpdateScaleText();
        }
    }

    //--------------------------------------------------------------------------------------
    // CURRENTLY UNUSED
    //--------------------------------------------------------------------------------------

    public void AddObjectToGravityZone(InteractibleObject obj)
    {
        if (!objectsInGravityZone.Contains(obj))
        {
            objectsInGravityZone.Add(obj);
        }
    }

    public void RemoveObjectFromGravityZone(InteractibleObject obj)
    {
        if (objectsInGravityZone.Contains(obj))
        {
            Debug.Log("Removing " + obj.gameObject.name);
            objectsInGravityZone.Remove(obj);
            UpdateScaleText();
        }
    }

    //--------------------------------------------------------------------------------------
    // END OF CURRENTLY UNUSED
    //--------------------------------------------------------------------------------------

    public void UpdateScaleText()
    {
        Debug.Log("Objects On Scale:");
        float weightSum = 0;
        foreach (InteractibleObject intObj in objectsOnScale)
        {
            Debug.Log(intObj.gameObject.name);
            weightSum += intObj.GetWeight();
        }
        if (weightSum > 0)
        {
            scaleText.text = "Weight:\n" + weightSum + " kg";
        }
        else
        {
            scaleText.text = "Place Object";
        }
    }
}
