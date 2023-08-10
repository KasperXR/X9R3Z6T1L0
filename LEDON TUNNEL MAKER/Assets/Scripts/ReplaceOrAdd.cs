using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceOrAdd : MonoBehaviour
{
    public ObjectRotator rotatorScript;
    public GameObject selectedObject;
    public bool insertON = false;
    public bool replaceON = false;
    public bool isMouseButtonReleased = false;
    private bool isColliderEnabled = true;
    public StartPartController startPartController;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    public GameObject selectedSnapPointNuts;
    public GameObject deleteButton;
    public List<DragAndSpawnObject> dragAndSpawnObjectScripts = new List<DragAndSpawnObject>();
    public List<GroupingObjects> GroupingObjects = new List<GroupingObjects>();
    private HierarchyBounds hierachySize;
    public RawImage replaceButtonImage;
    public RawImage insertButtonImage;
    private Indicator indicator;
    // Start is called before the first frame update
    void Start()
    {
        startPartController = GameObject.Find("CommandManager").GetComponent<StartPartController>();
        rotatorScript = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
        dragAndSpawnObjectScripts.AddRange(FindObjectsOfType<DragAndSpawnObject>());
        hierachySize = GameObject.Find("CommandManager").GetComponent<HierarchyBounds>();
        indicator = GameObject.Find("Indicator").GetComponent<Indicator>();
        // Store the original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            isMouseButtonReleased = false;
        }
        else  // Mouse button is not being held down
        {
            isMouseButtonReleased = true;
        }
        if (isMouseButtonReleased)
        {
            foreach (DragAndSpawnObject objScript in dragAndSpawnObjectScripts)
            {
                objScript.isCollidingWithReplacer = false;
            }
        }

        selectedObject = rotatorScript.selectedObject;
        

        if (selectedObject != null && (insertON || replaceON))
        {
            transform.position = selectedObject.transform.position;
            transform.rotation = selectedObject.transform.rotation;

            if (!isColliderEnabled)
            {
                GetComponent<BoxCollider>().enabled = true;
                isColliderEnabled = true;
            }
        }
        else
        {
            // Reset to the original position and rotation
            transform.position = originalPosition;
            transform.rotation = originalRotation;

            if (isColliderEnabled)
            {
                GetComponent<BoxCollider>().enabled = false;
                isColliderEnabled = false;
            }
        }

       

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 && replaceON)
        {
            Transform snapPointChild = selectedObject.transform.Find("SnapPoint");
            foreach (DragAndSpawnObject objScript in dragAndSpawnObjectScripts)
            {
                objScript.isCollidingWithReplacer = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            foreach (DragAndSpawnObject objScript in dragAndSpawnObjectScripts)
            {
                objScript.isCollidingWithReplacer = false;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            //Replacing the objects
            if (replaceON && isMouseButtonReleased)
            {
                StartCoroutine(ReplaceLogicCoroutine());
                
            }

            if (insertON && isMouseButtonReleased)
            {
                StartCoroutine(InsertLogicCoroutine());
                
            }

        }
        IEnumerator ReplaceLogicCoroutine()
        {
            Transform selectedObjectParent = selectedObject.transform.parent;
            Vector3 selectedObjectRotation = selectedObject.transform.localRotation.eulerAngles;
            Transform snapPointChild = selectedObject.transform.Find("SnapPoint");
            List<Transform> snapPointChildren = new List<Transform>();

            for (int i = 0; i < snapPointChild.childCount; i++)
            {
                Transform child = snapPointChild.GetChild(i);
                snapPointChildren.Add(child);
                child.SetParent(null);
            }

            selectedObject.transform.SetParent(null);
            Destroy(selectedObject);
            yield return new WaitForSeconds(0.5f);

            Transform otherParent = other.transform.parent;
            other.transform.SetParent(selectedObjectParent);
            
            if (other.transform.parent != null)
            {
                Debug.Log("DraggedObject position resetted");
                other.transform.localPosition = Vector3.zero;
                if (other.CompareTag("Straight") && other.transform.parent.parent.CompareTag("Curved"))
                {
                    float counterRotationY = -other.transform.parent.parent.localRotation.eulerAngles.y;
                    other.transform.localRotation = Quaternion.Euler(0, counterRotationY, 0);
                }
                else
                {
                    //other.transform.localRotation = Quaternion.identity;
                    other.transform.localRotation = Quaternion.Euler(selectedObjectRotation);
                    
                }

                other.gameObject.layer = LayerMask.NameToLayer("SnappedObjects");
                
            }
            if (other.transform.parent == null)
            {
                other.transform.SetParent(selectedObjectParent);
                other.transform.localRotation = Quaternion.Euler(selectedObjectRotation);
            }
            Destroy(selectedObject);
            Transform otherSnapPointChild = other.transform.Find("SnapPoint");
            for (int i = 0; i < snapPointChildren.Count; i++)
            {
                Transform child = snapPointChildren[i];
                child.SetParent(otherSnapPointChild);
                Vector3 childEuler = child.localEulerAngles;
                childEuler.x = 0f;
                childEuler.z = 0f;
                child.localEulerAngles = childEuler;
                child.localPosition = Vector3.zero;
            }

            
            selectedObject = null;
            startPartController.SaveData();
            hierachySize.CalculateBounds();
            indicator.MoveToLastSnapPoint();
            replaceButtonImage.color = Color.white;
            replaceON = false;
            //rotatorScript.SelectObject(other.gameObject); Fix this when have time
            

        }

        IEnumerator InsertLogicCoroutine()
        {
            Transform originalParent = selectedObject.transform.parent;
            Transform snapPointChildOfOther = other.transform.Find("SnapPoint");
            Vector3 originalSelectedRotation = selectedObject.transform.localRotation.eulerAngles;
            if (!snapPointChildOfOther.transform.IsChildOf(selectedObject.transform))
            {
                selectedObject.transform.SetParent(null);
                yield return new WaitForSeconds(0.5f);

                other.transform.SetParent(originalParent.transform);
                other.transform.localPosition = Vector3.zero;
                other.transform.localRotation = Quaternion.identity;

                selectedObject.transform.SetParent(snapPointChildOfOther.transform);
                selectedObject.transform.localPosition = Vector3.zero;
                selectedObject.transform.localRotation = Quaternion.identity;
                other.transform.localRotation = Quaternion.Euler(originalSelectedRotation);
                if (other.CompareTag("Straight") && originalParent.CompareTag("Curved"))
                {
                    //float counterRotationY = -originalParent.localRotation.eulerAngles.y;
                    //other.transform.localRotation = Quaternion.Euler(0, counterRotationY, 0);
                }

                other.gameObject.layer = LayerMask.NameToLayer("SnappedObjects");

                startPartController.SaveData();
                StartCoroutine(CalculateIndicatorHierarchy());
                insertButtonImage.color = Color.white;
                insertON = false;
                rotatorScript.SelectObject(other.gameObject);
            }
        }
    }

        private void ChangeLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            ChangeLayerRecursive(child.gameObject, layer);
        }
    }


    public void OnInsertButtonClick()
    {
        if (!insertON)
        {
            insertON = true;
            replaceON = false;
            replaceButtonImage.color = Color.white;
            insertButtonImage.color = Color.green;
        }
        else
        {
            insertON = false;
            replaceButtonImage.color = Color.white;
            insertButtonImage.color = Color.white;
        }
    }
    public void OnReplaceButtonClick()
    {
        if (!replaceON)
        {
            replaceON = true;
            insertON = false;
            replaceButtonImage.color = Color.green;
            insertButtonImage.color = Color.white;

        }
        else
        {
            replaceON = false;
            replaceButtonImage.color = Color.white;
            insertButtonImage.color = Color.white;
        }

    }
    public void DestroySelectedObject()
    {
        if (selectedObject != null && !selectedObject.name.StartsWith("Pole") && !selectedObject.name.StartsWith("Ending"))
        {
            Transform parent = selectedObject.transform.parent;
            
            Transform snapPoint = selectedObject.transform.Find("SnapPoint"); // find snapPoint child
            if (snapPoint != null)
            {
                int childCount = snapPoint.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    Transform child = snapPoint.GetChild(i);
                    if (child.name == "SnapPoint_nuts")
                    {
                        continue; // skip this child and move on to the next one
                    }
                    child.parent = parent; // assign parent to the detached child
                    Vector3 childEuler = child.localEulerAngles; // get the euler angles of the child's local rotation
                    childEuler.x = 0f; // reset X axis rotation to 0
                    childEuler.z = 0f; // reset Z axis rotation to 0
                    child.localEulerAngles = childEuler; // set the child's local rotation to the updated euler angles
                    child.localPosition = Vector3.zero; // set local position to zero
                }
            }
            if (parent != null)
            {
                Collider parentCollider = parent.GetComponent<Collider>();
                if (parentCollider != null)
                {
                    parentCollider.enabled = true;
                }
                selectedObject.transform.parent = null; // detach from parent
            }
            Destroy(selectedObject);
            StartCoroutine(CalculateIndicatorHierarchy());


        }
        else if (selectedObject != null && selectedObject.name.StartsWith("Pole"))
        {
            Destroy(selectedObject.transform.parent.gameObject);
        }
        else if(selectedObject != null && selectedObject.name.StartsWith("Ending"))
        {
            Destroy(selectedObject);
            StartCoroutine(CalculateIndicatorHierarchy());
        }

    }
    IEnumerator CalculateIndicatorHierarchy()
    {
        yield return new WaitForSeconds(0.1f);
        hierachySize.CalculateBounds();
        indicator.MoveToLastSnapPoint();
    }

}