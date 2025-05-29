using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(OVRRaycaster))]
public class MultiCameraCanvas : MonoBehaviour
{
    public Camera[] targetCameras;
    public float distanceFromPlayer = 2f;
    public Vector3 offset = new Vector3(0, 0, 0);

    private Camera canvasCamera;
    private OVRRaycaster ovrRaycaster;

    void Awake()
    {
        // Create a dedicated camera for the canvas
        var canvasCameraObj = new GameObject("CanvasCamera");
        canvasCamera = canvasCameraObj.AddComponent<Camera>();
        canvasCameraObj.transform.SetParent(transform);
        canvasCameraObj.transform.localPosition = Vector3.zero;
        canvasCameraObj.transform.localRotation = Quaternion.identity;

        // Configure the canvas camera
        canvasCamera.clearFlags = CameraClearFlags.Depth;
        canvasCamera.cullingMask = 1 << gameObject.layer;
        canvasCamera.depth = -1;

        // Set up the canvas
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = canvasCamera;
        canvas.planeDistance = 1;

        // Get or add OVRRaycaster
        ovrRaycaster = GetComponent<OVRRaycaster>();
        if (ovrRaycaster == null)
        {
            ovrRaycaster = gameObject.AddComponent<OVRRaycaster>();
        }

        // Position the canvas in front of the main camera
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            transform.position = mainCamera.transform.position + mainCamera.transform.forward * distanceFromPlayer + offset;
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }

    void Update()
    {
        // Update canvas position to follow the main camera
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            transform.position = mainCamera.transform.position + mainCamera.transform.forward * distanceFromPlayer + offset;
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
} 