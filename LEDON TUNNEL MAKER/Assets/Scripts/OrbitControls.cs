using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitControls : MonoBehaviour
{
    [SerializeField]
    private float sensitivity = 0.5f;
    [SerializeField]
    private float zoomSpeed = 10f;
    [SerializeField]
    private float minZoomDistance = 5f;
    [SerializeField]
    private float maxZoomDistance = 50f;
    public float cameraParentYPosition = 1f;
    
    private Vector3 prevMousePos;
    private Transform mainCamParent;
    [SerializeField]
    public bool isGuiDragging = false;
    [SerializeField]
    private List<DragAndSpawnObject> dragAndSpawnObjectScripts = new List<DragAndSpawnObject>();
    public bool anyObjectMoving = false;
    private Coroutine lerpCoroutine;
    public bool cameraRotationActive = false;
    public bool canCameraMove;
    public DragGUIMover dragGUIMover;
    public float uiOffset = 0.20f;

    private void Awake()
    {
        mainCamParent = Camera.main.transform.parent;

        DragAndSpawnObject[] dragAndSpawnObjects = FindObjectsOfType<DragAndSpawnObject>();

        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();

        if (audioListeners.Length > 1)
        {
            for (int i = 1; i < audioListeners.Length; i++)
            {
                Destroy(audioListeners[i]);
            }

            Debug.Log("Extra AudioListeners have been destroyed.");
        }

    }
    void Start()
    {

    }

    private void Update()
    {
        anyObjectMoving = false;
        foreach (DragAndSpawnObject dragAndSpawnObjectScript in dragAndSpawnObjectScripts)
        {
            if (dragAndSpawnObjectScript.objectMoving)
            {
                anyObjectMoving = true;
                break;
            }
        }

        if (Input.GetMouseButtonDown(1) && !dragGUIMover.isDraggingGUI)
        {
            prevMousePos = Input.mousePosition;
            prevMousePos.x /= Screen.width;
            prevMousePos.y /= Screen.height;
        }

        else if (Input.GetMouseButton(1) && !dragGUIMover.isDraggingGUI)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.x /= Screen.width;
            mousePos.y /= Screen.height;

            Vector3 prevMousePosScreen = new Vector3(prevMousePos.x * Screen.width, prevMousePos.y * Screen.height, prevMousePos.z);
            Vector3 mousePosScreen = new Vector3(mousePos.x * Screen.width, mousePos.y * Screen.height, mousePos.z);

            Vector3 deltaPos = (mousePosScreen - prevMousePosScreen) * 0.015f;

            // Use camera's right vector for horizontal movement
            Vector3 horizontalDirection = Camera.main.transform.right * -deltaPos.x;

            // Use camera's up vector for vertical movement
            Vector3 verticalDirection = Camera.main.transform.up * -deltaPos.y;

            Vector3 moveDirection = horizontalDirection + verticalDirection;

            mainCamParent.position += moveDirection;

            Vector3 pos = mainCamParent.position;
            pos.x = Mathf.Clamp(pos.x, -12f, 12f);
            pos.z = Mathf.Clamp(pos.z, -12f, 12f);
            pos.y = Mathf.Clamp(pos.y, -12f, 12f);
            mainCamParent.position = pos;

            prevMousePos = mousePos;
        }


        canCameraMove = true;
        foreach (OnUI onUiScript in FindObjectsOfType<OnUI>())
        {
            if (onUiScript.UiEntered)
            {
                canCameraMove = false;
                break;
            }
        }

        if (!anyObjectMoving && canCameraMove && !isGuiDragging)
        {
            if (Input.GetMouseButtonDown(0) && !dragGUIMover.isDraggingGUI)
            {
                prevMousePos = Input.mousePosition;
                prevMousePos.x /= Screen.width;
                prevMousePos.y /= Screen.height;
                cameraRotationActive = true;
            }
            else if (Input.GetMouseButton(0) && !dragGUIMover.isDraggingGUI && cameraRotationActive)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.x /= Screen.width;
                mousePos.y /= Screen.height;

                Vector2 deltaPos = (mousePos - prevMousePos) * sensitivity;

                Vector3 rot = mainCamParent.localEulerAngles;
                while (rot.x > 180f)
                    rot.x -= 360f;
                while (rot.x < -180f)
                    rot.x += 360f;

                rot.x = Mathf.Clamp(rot.x - deltaPos.y, -89.8f, 89.8f);
                rot.y += deltaPos.x;
                rot.z = 0f;

                mainCamParent.localEulerAngles = rot;
                prevMousePos = mousePos;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                cameraRotationActive = false;
            }

            float scrollValue = Input.GetAxis("Mouse ScrollWheel");
            if (scrollValue != 0 && !dragGUIMover.isDraggingGUI)
            {
                if (Camera.main.orthographic)
                {
                    minZoomDistance = 2f;
                    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - scrollValue * zoomSpeed, minZoomDistance, maxZoomDistance);
                }
                else
                {
                    minZoomDistance = 5f;
                    Vector3 direction = mainCamParent.position - Camera.main.transform.position;
                    float currentDistance = Vector3.Distance(mainCamParent.position, Camera.main.transform.position);
                    float targetDistance = currentDistance - scrollValue * zoomSpeed;
                    targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);

                    Camera.main.transform.position = mainCamParent.position - direction.normalized * targetDistance;
                }
            }

        }

        if ((float)Screen.width / Screen.height != 16f / 9f)
        {
            float targetAspect = 16f / 9f;
            float currentAspect = (float)Screen.width / Screen.height;

            float scaleHeight = currentAspect / targetAspect;
            float scaleWidth = 1f / scaleHeight;

            if (scaleHeight < 1f)
            {
                Camera.main.rect = new Rect(0f, (1f - scaleHeight) / 2f, 1f, scaleHeight);
            }
            else
            {
                Camera.main.rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
            }
        }
        else
        {
            Camera.main.rect = new Rect(0f, 0f, 1f, 1f);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameObject startPart = GameObject.Find("StartPart");
            if (startPart != null)
            {
                StartCoroutine(LerpToViewObject(startPart));
            }
            else
            {
                Debug.LogError("No GameObject named 'StartPart' found.");
            }
        }
    }

    public void LerpToTargetHeight(float targetHeight)
    {
        if (lerpCoroutine != null)
        {
            StopCoroutine(lerpCoroutine);
        }

        lerpCoroutine = StartCoroutine(LerpToHeightCoroutine(targetHeight));
    }

    private IEnumerator LerpToHeightCoroutine(float targetHeight)
    {
        float lerpDuration = 1f;
        float startTime = Time.time;
        Vector3 startPosition = mainCamParent.position;
        Vector3 targetPosition = new Vector3(mainCamParent.position.x, targetHeight, mainCamParent.position.z);

        while (Time.time - startTime < lerpDuration)
        {
            float t = (Time.time - startTime) / lerpDuration;
            mainCamParent.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        mainCamParent.position = targetPosition;
    }
    private IEnumerator LerpToViewObject(GameObject targetObject)
    {
        float lerpDuration = 1f;
        float startTime = Time.time;
        Vector3 startPosition = mainCamParent.position;

        Bounds bounds = CalculateObjectBounds(targetObject);
        Vector3 targetCenter = bounds.center;

        // Calculate target position of the camera parent
        Vector3 targetPosition = startPosition;

        // Always adjust X, Y and Z positions
        targetPosition.x = targetCenter.x;
        targetPosition.z = targetCenter.z;
        targetPosition.y = targetCenter.y - mainCamParent.GetChild(0).localPosition.y;

        // Perform smooth transition to the target position
        while (Time.time - startTime < lerpDuration)
        {
            float t = (Time.time - startTime) / lerpDuration;
            mainCamParent.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        mainCamParent.position = targetPosition;
    }




    private bool IsCameraLookingFromTop()
    {
        // Modify this condition based on how you determine whether the camera is looking from the top
        return Vector3.Dot(mainCamParent.up, Vector3.up) > 0.5f;
    }
    private Bounds CalculateObjectBounds(GameObject rootObject)
    {
        Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(rootObject.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
    private float CalculateRequiredDistance(Bounds bounds, float cameraAngle, float aspectRatio, Vector3 cameraOffset)
    {
        float objectSize = bounds.extents.magnitude;
        float cameraSize = objectSize / (2.0f * Mathf.Tan(0.5f * cameraAngle * Mathf.Deg2Rad));
        return cameraSize + cameraOffset.magnitude;
    }
    private float CalculateRequiredDistance(Bounds bounds, float cameraAngle, float aspectRatio)
    {
        float objectSize = bounds.extents.magnitude;
        float cameraSize = objectSize / (2.0f * Mathf.Tan(0.5f * cameraAngle * Mathf.Deg2Rad));
        return cameraSize;
    }
}
