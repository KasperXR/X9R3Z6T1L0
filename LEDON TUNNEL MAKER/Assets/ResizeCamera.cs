using UnityEngine;

public class ResizeCamera : MonoBehaviour
{
    public Canvas myCanvas; // Assign this in the Inspector
    private Camera myCamera;

    private void Start()
    {
        myCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        RectTransform canvasRect = myCanvas.GetComponent<RectTransform>();
        myCamera.orthographicSize = canvasRect.sizeDelta.y / 2;
    }
}
