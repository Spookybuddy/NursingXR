using UnityEngine;
using System.Collections;

public class DeviceCameraController : MonoBehaviour
{
    [HideInInspector]
    public WebCamTexture cameraTexture;

    private GameObject e_CameraPlaneObj;
    private WaitForSeconds wfs;
    private Coroutine updateCoroutine;

    private Renderer ren;

    public bool IsPlaying
    {
        get
        {
            if (cameraTexture == null)
                return false;
            else
                return cameraTexture.isPlaying;
        }
    }

    // Use this for initialization  
    void Awake()
    {
        wfs = new WaitForSeconds(.2f);

        e_CameraPlaneObj = GetComponentInChildren<CameraPlaneController>(true).gameObject;
    }

    private void OnEnable()
    {
        updateCoroutine = StartCoroutine(UpdateCoroutine());
    }

    private void OnDisable()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);

            updateCoroutine = null;
        }
    }


    private IEnumerator UpdateCoroutine()
    {
        // todo: we'd want this to start and stop at the correct time too...
        while (true)
        {
            yield return wfs;

            ApplyCameraTexture();
        }
    }

    void ApplyCameraTexture()
    {
        if (IsPlaying)
        {
            if (e_CameraPlaneObj.activeSelf)
            {
                if (ren == null)
                {
                    ren = e_CameraPlaneObj.GetComponent<Renderer>();
                }

                ren.material.mainTexture = cameraTexture;
            }
        }
    }

    IEnumerator CamCon()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        cameraTexture = new WebCamTexture(devices[0].name);

        // rend.material.mainTexture = cameraTexture;
        cameraTexture.Play();

        yield return null;
    }

    /// <summary>
    /// Stops the work.
    /// when you need to leave current scene ,you must call this func firstly
    /// </summary>
    public void StopWork()
    {
        if (this.cameraTexture != null &&
            this.cameraTexture.isPlaying)
        {
            this.cameraTexture.Stop();
            Destroy(this.cameraTexture);
            this.cameraTexture = null;
        }

        if (e_CameraPlaneObj != null)
        {
            e_CameraPlaneObj.GetComponent<Renderer>().material.mainTexture = cameraTexture;
        }
    }

    public void StartWork()
    {
        if (IsPlaying)
        {
            StopWork();
        }

        StartCoroutine(CamCon());
    }
}