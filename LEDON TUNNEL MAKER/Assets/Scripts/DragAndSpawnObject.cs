using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class DragAndSpawnObject : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public GameObject prefab;
    public bool objectMoving = false;
    public bool isDragging = false;
    private bool isWithinUI = true;
    public GameObject spawnedObject;
    private Vector3 mOffset;
    private float mZCoord;
    public bool snappedToOtherObject = false;
    public bool snapped = false;
    public OrbitControls orbitControlsScript;
    private float lastClickTime = 0f;
    private float clickInterval = 0.5f;
    private int clickCount = 0;
    public StartPartController startPartController;
    public bool isCollidingWithReplacer = false;
    private HierarchyBounds hierachySize;
    private bool isColliding = false;
    private Indicator indicator;
    private ObjectRotator rotator;

    void Start()
    {
        startPartController = GameObject.Find("CommandManager").GetComponent<StartPartController>();
        hierachySize = GameObject.Find("CommandManager").GetComponent<HierarchyBounds>();
        orbitControlsScript = GameObject.Find("OrbitCamera").GetComponent<OrbitControls>();
        indicator = GameObject.Find("Indicator").GetComponent<Indicator>();
        rotator = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
    }

    void Update()
    {
        if (transform.parent != null && transform.parent.name == "SnapPoint")
        {
            enabled = false;
            snappedToOtherObject = true;
            return;
        }

        if (Time.time - lastClickTime > clickInterval && clickCount > 0)
        {
            clickCount = 0;
            lastClickTime = 0;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        isColliding = true;
        Debug.Log("DraggedObject is colliding");
    }

    private void OnTriggerExit(Collider other)
    {
        isColliding = false;
        Debug.Log("DraggedObject is not colliding");
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            clickCount++;

            if (clickCount == 1)
            {
                lastClickTime = Time.time;
            }

            if (clickCount > 1 && Time.time - lastClickTime < clickInterval)
            {
                // Double click detected
                GameObject startPart = GameObject.Find("StartPart");
                if (startPart != null)
                {
                    Transform furthestGrandChild = GetFurthestGrandchild(startPart.transform);
                    Debug.Log("furthers" + furthestGrandChild);
                    if (furthestGrandChild != null && !furthestGrandChild.parent.name.StartsWith("Ending") && !furthestGrandChild.Find("Ending_End"))
                    {

                        GameObject newObject = Instantiate(prefab);
                        if (newObject.name.StartsWith("Ending-"))
                        {
                            Destroy(newObject);
                            return;
                        }
                        newObject.name = prefab.name;
                        newObject.transform.SetParent(furthestGrandChild);
                        newObject.transform.localPosition = Vector3.zero;
                        newObject.transform.localScale = Vector3.one;
                        
                        
                        newObject.transform.localRotation = Quaternion.identity;
                        
                        
                        newObject.layer = LayerMask.NameToLayer("SnappedObjects");
                        startPartController.SaveData();
                        hierachySize.CalculateBounds();
                        indicator.MoveToLastSnapPoint();
                        rotator.SelectObject(newObject);


                    }
                    else if(furthestGrandChild != null && furthestGrandChild.parent.name.StartsWith("Ending") && !furthestGrandChild.Find("Ending_End"))
                    {
                        GameObject newObject = Instantiate(prefab);
                        if (!newObject.name.StartsWith("Ending")){
                            Destroy( newObject );
                            return;
                        }
                        newObject.name = prefab.name;
                        newObject.transform.SetParent(furthestGrandChild);
                        newObject.transform.localPosition = Vector3.zero;
                        newObject.transform.localScale = Vector3.one;

                        newObject.transform.localRotation = Quaternion.identity;


                        newObject.layer = LayerMask.NameToLayer("SnappedObjects");
                        startPartController.SaveData();
                        hierachySize.CalculateBounds();
                        indicator.MoveToLastSnapPoint();
                        rotator.SelectObject(newObject);
                        orbitControlsScript.enabled = true;
                    }
                }

                clickCount = 0;
                lastClickTime = 0;
            }
            else if (clickCount > 1)
            {
                clickCount = 1;
                lastClickTime = Time.time;
            }
            else
            {
                mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
                mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
        
                isDragging = true;
                isWithinUI = true;
                snappedToOtherObject = false;
                objectMoving = true;
            }
        }
        else
        {
            // Reset the clickCount and lastClickTime variables
            clickCount = 0;
            lastClickTime = 0;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        orbitControlsScript.enabled = false;
        if (isDragging && !isCollidingWithReplacer)
        {
            // Ignore raycast against dragged object layer
            int layerMask = ~(1 << LayerMask.NameToLayer("DraggedObject") | 1 << LayerMask.NameToLayer("Ignore Raycast"));

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1.0f);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 worldPosition = hit.point;
                Quaternion rotation = prefab.transform.rotation;
                /*
                if (prefab.tag == "Straight")
                {
                    
                    // Set the rotation of the straight piece to match the CameraParent
                    Vector3 lookDir = Camera.main.transform.parent.forward;
                    lookDir.y = 0;
                    rotation = Quaternion.LookRotation(lookDir);

                    // Rotate the straight piece by 90 degrees around the z-axis
                    if(prefab.name != "Ending")
                    {
                        rotation *= Quaternion.Euler(0f, -90f, 90f);
                    }
                    else
                    {
                        rotation *= Quaternion.Euler(180f, -90f, 90f);
                    }

                    // Add an offset of 1 point to the right of the hit point's x-coordinate
                    if(prefab.name == "Straight-106")
                    {
                        Vector3 offset = new Vector3(0f, 1f, 0f);
                        worldPosition += offset;
                    }
                    else if (prefab.name == "Straight-59")
                    {
                        Vector3 offset = new Vector3(0f, 1f, 0f);
                        worldPosition += offset;
                    }
                    else if (prefab.name == "Straight-35")
                    {
                        Vector3 offset = new Vector3(0f, 1f, 0f);
                        worldPosition += offset;
                    }


                    // Keep the object at the same height above the ground
                    float heightOffset = 0.45f;
                    float originalY = hit.point.y;
                    worldPosition = new Vector3(worldPosition.x, originalY + heightOffset, worldPosition.z);
                }
                */
                if (isWithinUI && eventData.button == PointerEventData.InputButton.Left)
                {
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        GetComponent<RectTransform>(),
                        eventData.position,
                        eventData.pressEventCamera,
                        out localPoint
                    );

                    if (!GetComponent<RectTransform>().rect.Contains(localPoint))
                    {
                        isWithinUI = false;

                        spawnedObject = Instantiate(prefab, worldPosition, rotation);
                        spawnedObject.layer = LayerMask.NameToLayer("DraggedObject");
                        spawnedObject.name = prefab.name;

                      
                    }
                }
                else
                {
                    // Ignore the object being dragged
                    if (hit.transform.gameObject == gameObject)
                    {
                        return;
                    }

                    worldPosition.y = Mathf.Max(worldPosition.y, 0f);
                    if (spawnedObject == null && eventData.button == PointerEventData.InputButton.Left)
                    {
                        spawnedObject = Instantiate(prefab, worldPosition, rotation);
                        spawnedObject.name = prefab.name;
                        startPartController.SaveData();
                        hierachySize.CalculateBounds();
                        indicator.MoveToLastSnapPoint();

                    }
                    else if (spawnedObject != null && eventData.button == PointerEventData.InputButton.Left && hit.transform != null)
                    {
                        spawnedObject.transform.position = worldPosition;
                    }
                }
            }
        }
    }
   

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isDragging = false;
            objectMoving = false;
            orbitControlsScript.enabled = true;
            // Cast a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;

            // Define a layer mask to include only the Plane layer
            int layerMask = LayerMask.GetMask("Plane");

            // Perform raycast with the specified layer mask
            hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

            bool releasedOnPlane = false;

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Plane"))
                {
                    releasedOnPlane = true;
                    break;
                }
            }

            // Define a layer mask to include only the SnappedObjects layer
            int snappedObjectsLayerMask = LayerMask.GetMask("SnappedObjects");

            // Perform raycast with the specified layer mask
            RaycastHit snappedObjectsHit;
            bool releasedOnSnappedObject = Physics.Raycast(ray, out snappedObjectsHit, Mathf.Infinity, snappedObjectsLayerMask);

            if (releasedOnPlane && !snappedToOtherObject && !releasedOnSnappedObject && (spawnedObject == null || spawnedObject.transform.parent == null) && !isCollidingWithReplacer)
            {
                
                Destroy(spawnedObject);
            }
            else
            {
                // If the object is released not on the plane, reset the snapped flags
                snappedToOtherObject = false;
                snapped = false;
            }

            //Debug.Log("Snapped: " + snapped);
            //Debug.Log("Snapped to Other Object: " + snappedToOtherObject);
            //Debug.Log("Object Parent: " + (spawnedObject != null ? spawnedObject.transform.parent : null));

            
            spawnedObject = null;
            
        }
    }
    
    
    private Vector3 GetMouseAsWorldPoint()
    {
        // Calculate the distance from the camera to the object
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);

        // Cast a ray from the camera through the mouse position, at the calculated distance
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPosition = ray.GetPoint(distance);

        // Clamp the y position to 0 or above
        worldPosition.y = Mathf.Max(worldPosition.y, 0f);

        return worldPosition;
    }

    private Transform GetFurthestGrandchild(Transform parent)
    {
        Transform furthestSnapPoint = null;
        int maxDepth = -1;
        foreach (Transform child in parent)
        {
            if (child.name == "SnapPoint")
            {
                int childDepth = GetHierarchyDepth(child);

                if (childDepth > maxDepth)
                {
                    maxDepth = childDepth;
                    furthestSnapPoint = child;
                }
            }

            Transform grandChild = GetFurthestGrandchild(child);
            if (grandChild != null)
            {
                int grandChildDepth = GetHierarchyDepth(grandChild);
                if (grandChildDepth > maxDepth)
                {
                    maxDepth = grandChildDepth;
                    furthestSnapPoint = grandChild;
                }
            }
        }

        return furthestSnapPoint;
    }

    private int GetHierarchyDepth(Transform t)
    {
        int depth = 0;
        while (t.parent != null)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }
}
