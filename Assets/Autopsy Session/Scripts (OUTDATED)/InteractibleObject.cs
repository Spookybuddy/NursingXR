//--------------------------------------------------------------------------------------
// InteractibleObject
//
// Script to be attached to objects that represent various body parts of the person
// being autopsied.
//
// Each object needs to have an associated body system (and layer value within),
// body section, and weight (in kilograms).
//--------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractibleObject : MonoBehaviour
{
    [SerializeField]
    private BodySection bodySection;
    [SerializeField]
    private BodySystem bodySystem;
    [SerializeField]
    private int systemLayer;
    [SerializeField]
    private float weight;

    private CameraControl camControl;
    private ScaleBehavior scaleBehavior;
    private Rigidbody rb;
    private Collider hitBox;
    private bool inGravityZone = false, isCollidingWithScale = false,
                 onScaleButInactive = false;
    private Vector3 defaultPos;
    private Quaternion defaultRot;

    // Start is called before the first frame update
    void Start()
    {
        defaultPos = transform.position;
        defaultRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
        hitBox = GetComponent<Collider>();
        camControl = GameObject.Find("Main Camera").GetComponent<CameraControl>();
        scaleBehavior = GameObject.Find("Scale").GetComponent<ScaleBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //--------------------------------------------------------------------------------------
    // Getters and Setters
    //--------------------------------------------------------------------------------------

    public BodySection GetBodySection()
    {
        return bodySection;
    }

    public BodySystem GetBodySystem()
    {
        return bodySystem;
    }

    public int GetSystemLayer()
    {
        return systemLayer;
    }

    public float GetWeight()
    {
        return weight;
    }

    public bool IsOnScaleButInactive()
    {
        return onScaleButInactive;
    }

    public void SetOnScaleButInactive(bool input)
    {
        onScaleButInactive = input;
    }


    // Called when the object is selected by the user
    public void OnSelection()
    {
        ObeyPhysics(false);
    }

    // Called when the object is released by the user
    public void OnRelease()
    {
        if (inGravityZone)
        {
            ObeyPhysics(true);
        }
    }

    // Turns on/off the physics for this object 
    public void ObeyPhysics(bool isObeying)
    {
        hitBox.isTrigger = !isObeying;
        rb.useGravity = isObeying;
        if (!isObeying)
        {
            rb.velocity = Vector3.zero;
        }
    }

    // Resets the object back to it's starting state
    public void ResetObject()
    {
        transform.position = defaultPos;
        transform.rotation = defaultRot;
        hitBox.isTrigger = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        onScaleButInactive = false;
        scaleBehavior.RemoveObjectFromScale(this);
        scaleBehavior.RemoveObjectFromGravityZone(this);
    }

    //--------------------------------------------------------------------------------------
    // Collision and Trigger Functions
    //--------------------------------------------------------------------------------------

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GravityZone"))
        {
            inGravityZone = true;
            scaleBehavior.AddObjectToGravityZone(this);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("GravityZone"))
        {
            inGravityZone = false;
            scaleBehavior.RemoveObjectFromGravityZone(this);
            ObeyPhysics(false);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log(gameObject.name + " -> " + collision.gameObject.name + " " + collision.GetContact(0).normal);
        if (collision.GetContact(0).normal.y > 0)
        {
            Debug.Log("Adding " + gameObject.name);
            scaleBehavior.AddObjectToScale(this);
            if (collision.gameObject.CompareTag("Scale"))
            {
                isCollidingWithScale = true;
            }
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        Debug.Log("Collision Exit "+ gameObject.name + " " + rb.velocity.magnitude + " " + (rb.velocity.magnitude > .01f));
        if ((!isCollidingWithScale || collision.gameObject.CompareTag("Scale")) &&
            (rb.velocity.magnitude > .01f || camControl.GetSelectedObject() == gameObject))
        {
            scaleBehavior.RemoveObjectFromScale(this);
            isCollidingWithScale = false;
        }
    }
}
