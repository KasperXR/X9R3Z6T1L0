using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMatcher : MonoBehaviour
{
    private Camera mainCamera;
    private Camera lineCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = transform.parent.GetComponent<Camera>();
        lineCamera = transform.GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        lineCamera.orthographic = mainCamera.orthographic;
        lineCamera.orthographicSize = mainCamera.orthographicSize;
    }

}
