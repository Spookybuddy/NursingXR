//--------------------------------------------------------------------------------------
// CameraControl
//
// Simple system designed for rotating a camera around based on tap/click input.
//
// Also handles the movement of interactible objects
//--------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
    private Camera cam;
    private bool isDraggingCamera, isDraggingObject;
    private Vector3 curMouseViewportPos, lastMouseViewportPos;
    private float objectDragAreaHeight;
    private GameObject selectedObject;
    private float camSpeed = .3f;

    // Represents the amount the camera will rotate by dragging
    // the mouse across the full screen horizontally.
    private float xScreenDegreeRatio = 80;

    // The height of the screen relative to it's width
    private float yHeightRelative;

    [SerializeField]
    private Transform normalDefiner, objDragPlaneX, objDragPlaneY;
    [SerializeField]
    private Canvas canvasUI;
    [SerializeField]
    private MenuManager menuManager;
    [SerializeField]
    private GraphicRaycaster uiRaycaster;
    [SerializeField]
    private EventSystem mainEventSystem;
    /*[SerializeField]
    private TMPro.TextMeshProUGUI debugText;*/

    private bool movingLeft, movingRight, movingForwards, movingBack;
    private PointerEventData pointerEventData;
    

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        yHeightRelative = 1 / cam.aspect;
    }

    // Update is called once per frame
    void Update()
    {
        // Portion that handles camera rotation, object dragging, and menu closing
        if (Input.GetMouseButtonDown(0) && IsMouseInWindow() && !IsMouseOverUI() && 
            !menuManager.InMenuAnimation() && !menuManager.IsMenuOpen())
        {
            bool hitObj = Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
            if (hitObj && hitInfo.collider.gameObject.GetComponent<InteractibleObject>() != null)
            {
                selectedObject = hitInfo.collider.gameObject;
                Plane viewportPlaneAtCam = new Plane(-(normalDefiner.position - transform.position).normalized, transform.position);
                float objDistanceToPlane = viewportPlaneAtCam.GetDistanceToPoint(selectedObject.transform.position);
                objectDragAreaHeight = 2 * objDistanceToPlane / Mathf.Sqrt(3);
                isDraggingObject = true;
                lastMouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);
                curMouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);
                selectedObject.GetComponent<InteractibleObject>().OnSelection();
            }
            else if (!hitObj)
            {
                isDraggingCamera = true;
                lastMouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);
                curMouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonDown(0) && !IsMouseOverUI() &&
                 !menuManager.InMenuAnimation() && menuManager.IsMenuOpen())
        {
            menuManager.ToggleMenu();
        }
        /* else if (Input.GetMouseButtonDown(0))
        {
            Used for debugging
        }*/

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingCamera = false;
            if (isDraggingObject)
            {
                selectedObject.GetComponent<InteractibleObject>().OnRelease();
            }
            isDraggingObject = false;
        }

        if (isDraggingCamera)
        {
            lastMouseViewportPos = curMouseViewportPos;
            curMouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);
            if (lastMouseViewportPos != curMouseViewportPos)
            {
                float xDragDelta = curMouseViewportPos.x - lastMouseViewportPos.x;
                float yDragDelta = yHeightRelative * (curMouseViewportPos.y - lastMouseViewportPos.y);
                transform.rotation *= Quaternion.Euler(yDragDelta * xScreenDegreeRatio, -xDragDelta * xScreenDegreeRatio, 0);
            }
        }

        if (isDraggingObject)
        {
            lastMouseViewportPos = curMouseViewportPos;
            curMouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);
            if (lastMouseViewportPos != curMouseViewportPos)
            {
                float xDragDelta = curMouseViewportPos.x - lastMouseViewportPos.x;
                float yDragDelta = curMouseViewportPos.y - lastMouseViewportPos.y;
                Vector3 objDelta = xDragDelta * (objDragPlaneX.position - transform.position) * objectDragAreaHeight * cam.aspect +
                                   yDragDelta * (objDragPlaneY.position - transform.position) * objectDragAreaHeight;
                selectedObject.transform.Translate(objDelta);
            }
        }

        // Portion that handles non-rotation camera movement
        if (movingLeft)
        {
            transform.Translate(-(objDragPlaneX.position - transform.position) * camSpeed * Time.deltaTime, Space.World);
        }

        if (movingRight)
        {
            transform.Translate((objDragPlaneX.position - transform.position) * camSpeed * Time.deltaTime, Space.World);
        }

        if (movingForwards)
        {
            transform.Translate(-(normalDefiner.position - transform.position) * camSpeed * Time.deltaTime, Space.World);
        }

        if (movingBack)
        {
            transform.Translate((normalDefiner.position - transform.position) * camSpeed * Time.deltaTime, Space.World);
        }
    }

    // Returns true if the mouse is in the game window of the unity editor, false otherwise.
    private bool IsMouseInWindow()
    {
        Vector3 mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
        return mousePos.x >= 0 && mousePos.x <= 1 && mousePos.y >= 0 && mousePos.y <= 1;
    }

    private bool IsMouseOverUI()
    {
        pointerEventData = new PointerEventData(mainEventSystem);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        uiRaycaster.Raycast(pointerEventData, results);
        return results.Count > 0;
    }


    //--------------------------------------------------------------------------------------
    // Camera Strafe Setters
    //--------------------------------------------------------------------------------------

    public void SetLeftMovement(bool shouldMove)
    {
        movingLeft = shouldMove;
    }

    public void SetRightMovement(bool shouldMove)
    {
        movingRight = shouldMove;
    }

    public void SetForwardsMovement(bool shouldMove)
    {
        movingForwards = shouldMove;
    }

    public void SetBackMovement(bool shouldMove)
    {
        movingBack = shouldMove;
    }

    //--------------------------------------------------------------------------------------
    // Getters and Setters
    //--------------------------------------------------------------------------------------

    public GameObject GetSelectedObject()
    {
        return selectedObject;
    }

}
