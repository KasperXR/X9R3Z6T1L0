using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthographicCameraManager : MonoBehaviour
{
    public OrbitControls mainCameraOrbitControls;
    public Camera topViewCamera;
    public Camera leftViewCamera;
    public Camera rightViewCamera;
    public Camera frontViewCamera;
    public Camera backViewCamera;

    private void Start()
    {
        // Disable all cameras by default
        DisableAllCameras();
    }

    private void DisableAllCameras()
    {
        topViewCamera.enabled = false;
        leftViewCamera.enabled = false;
        rightViewCamera.enabled = false;
        frontViewCamera.enabled = false;
        backViewCamera.enabled = false;
    }

    // Call this method to enable the desired camera view and disable others
    public void EnableCameraView(Camera viewCamera)
    {
        DisableAllCameras();
        viewCamera.enabled = true;
    }

    // Implement methods for taking screenshots here
}