using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class GroupingObjects : MonoBehaviour
{
    public OrbitControls orbitControlsScript;
    public ReplaceOrAdd replaceOrAddScript;
    public bool inserting;
    public bool replacing;
    public StartPartController startPartController;
    private bool canParent;
    private BoxCollider boxCollider;
    private HierarchyBounds bounds;
    private Indicator indicator;
    private ObjectRotator rotator;

    private void Start()
    {
        orbitControlsScript = GameObject.Find("OrbitCamera").GetComponent<OrbitControls>();
        replaceOrAddScript = GameObject.Find("Replacer").GetComponent<ReplaceOrAdd>();
        startPartController = GameObject.Find("CommandManager").GetComponent<StartPartController>();
        bounds = GameObject.Find("CommandManager").GetComponent<HierarchyBounds>();
        indicator = GameObject.Find("Indicator").GetComponent<Indicator>();
        rotator = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        inserting = replaceOrAddScript.insertON;
        replacing = replaceOrAddScript.replaceON;
        if (Input.GetMouseButtonDown(0))
        {
            canParent = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            canParent = true;
        }
        if (boxCollider != null)
        {
            if (inserting || replacing && transform.childCount < 1 ||
                (transform.parent != null && transform.parent.name == "StartPart" && transform.childCount == 1) ||
                (transform.parent != null && (transform.parent.CompareTag("Straight") || transform.parent.CompareTag("Curved")) && transform.childCount >= 1))
            {
                boxCollider.enabled = false;
            }
            else
            {
                boxCollider.enabled = true;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (orbitControlsScript.anyObjectMoving == false && inserting == false && canParent && replacing == false)
        {
            if ((other.CompareTag("Straight") || other.CompareTag("Curved")) && other.gameObject.layer != 3)
            {
                other.transform.parent = transform;
                other.transform.localPosition = Vector3.zero;

                if (other.CompareTag("Straight") && transform.parent.name != "StartPart" && !transform.parent.CompareTag("Straight"))
                {
                    // Counter rotation on the local Y-axis
                    float counterRotationY = -transform.parent.localRotation.eulerAngles.y;
                    other.transform.localRotation = Quaternion.Euler(0, counterRotationY, 0);
                    other.transform.localScale = Vector3.one;
                    rotator.SelectObject(other.gameObject);
                    bounds.CalculateBounds();
                    indicator.MoveToLastSnapPoint();
                }
                else if (transform.parent.name.StartsWith("Ending") && !other.name.StartsWith("Ending"))
                {
                    Destroy(other.gameObject);

                }
                else if (!transform.parent.name.StartsWith("Ending") && other.name.StartsWith("Ending-"))
                {
                    Destroy(other.gameObject);
                }
                else
                {
                    other.transform.localRotation = Quaternion.identity;
                    other.transform.localScale = Vector3.one;
                    rotator.SelectObject(other.gameObject);
                    bounds.CalculateBounds();
                    indicator.MoveToLastSnapPoint();
                }

                // Change the layer of the snapped object and its children to layer 3
                other.gameObject.layer = 3;
                foreach (Transform child in other.transform)
                {
                    child.gameObject.layer = 3;
                }
               // startPartController.SaveData();
            }
        }
    }
}
