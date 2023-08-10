using Lean.Gui;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public class ObjectRotator : MonoBehaviour
{
    public float rotationAngle = 15f;
    public GameObject selectedObject;
    public bool isObjectSelected;
    public GameObject CurvedPrefab38;
    public GameObject CurvedPrefab45;
    public GameObject Straight106;
    public ReplaceOrAdd replacer;
    public LeanToggle windowsToggle;
    public bool bodyColorOn = true;
    public bool nutsColorON = false;
    public bool windowsColorOn = false;
    private Color originalColor;
    private Renderer selectedObjectRenderer;
    private Coroutine rotationCoroutine;
    private bool rotateLeft;
    private bool rotateRight;
    public StartPartController startPartController;
    public Material[] BodyMaterials;
    public Material[] NutsMaterials;
    public Material[] WindowMaterials;
    private Outline selectedOutline;
    private float mouseDownTime;
    private HierarchyBounds hierachySize;
    public ColorSwitcher bodyButton;
    public ColorSwitcher nutsButton;
    public ColorSwitcher windowsButton;
    private Indicator indicator;
    private void Start()
    {
        startPartController = GameObject.Find("CommandManager").GetComponent<StartPartController>();
        windowsToggle = GameObject.Find("WindowToggle").GetComponent<LeanToggle>();
        hierachySize = GameObject.Find("CommandManager").GetComponent<HierarchyBounds>();
        indicator = GameObject.Find("Indicator").GetComponent<Indicator>();
        bodyColorOn = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int layerMask = 1 << LayerMask.NameToLayer("SnappedObjects");
            layerMask |= 1 << LayerMask.NameToLayer("DraggedObject");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            mouseDownTime = Time.time;
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // Mouse is over UI, do not perform raycast or selection
                return;
            }
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject.name == "SnapPoint" || hit.collider.gameObject.tag == ("Buttons") || hit.collider.gameObject.tag == ("GUI") || hit.collider.gameObject.tag == ("Plane"))
                {
                    return;
                }
                else
                {
                    if (selectedObject != null)
                    {
                        // Disable outline on the previously selected object
                        selectedObject.GetComponent<Outline>().enabled = false;
                    }

                    selectedObject = hit.collider.gameObject;
                    isObjectSelected = true;
                    // Enable outline on the newly selected object
                    selectedObject.GetComponent<Outline>().enabled = true;
                    if (selectedObject.name.StartsWith("Pole"))
                    {
                        
                    }
                    //Debug.Log(selectedObject, selectedObject.transform);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // If the pointer is over a UI element, ignore the rest of the code
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Check if the elapsed time is short enough to consider this a click, not a drag
            if (Time.time - mouseDownTime < 0.2f)  // Adjust this value as needed
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    // Check if the hit object is the plane
                    if (hit.collider.gameObject.tag == "Plane")
                    {
                        if (selectedObject != null)
                        {
                            // Disable outline on the previously selected object
                            selectedObject.GetComponent<Outline>().enabled = false;
                        }

                        selectedObject = GameObject.Find("GUISave");
                        // Enable outline on the newly selected object
                        selectedObject.GetComponent<Outline>().enabled = true;
                    }
                }
                else
                {
                    if (selectedObject != null)
                    {
                        // Disable outline on the previously selected object
                        selectedObject.GetComponent<Outline>().enabled = false;
                    }

                    selectedObject = GameObject.Find("GUISave");
                    // Enable outline on the newly selected object
                }
            }
        }


        // Check if the user has pressed the A or D keys
        if (isObjectSelected && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
        {
            // Check if the selected object's name doesn't start with "Straight"
            if (!selectedObject.name.StartsWith("Straight"))
            {
                // Rotate the selected object around the Y-axis
                float direction = Input.GetKeyDown(KeyCode.LeftArrow) ? -rotationAngle : rotationAngle;
                selectedObject.transform.Rotate(Vector3.up, direction, Space.Self);
                hierachySize.CalculateBounds();
                indicator.MoveToLastSnapPoint();

                // Check if the selected object has a child with name starting "Straight"
                Transform snapPoint = selectedObject.transform.Find("SnapPoint");
                if (snapPoint != null)
                {
                    for (int i = 0; i < snapPoint.childCount; i++)
                    {
                        Transform child = snapPoint.GetChild(i);
                        if (child.name.StartsWith("Straight"))
                        {
                            // Rotate the child object around the Y-axis along with the selected object
                            //child.Rotate(Vector3.up, -direction, Space.Self);
                            break;
                        }
                    }
                }
                
                startPartController.SaveData();
                indicator.MoveToLastSnapPoint();
                hierachySize.CalculateBounds();
            }
        }
    }
    public void SelectObject(GameObject obj)
    {
        if (replacer.insertON == false && replacer.replaceON == false)
        {
            if (selectedObject != null)
            {
                // Disable outline on the previously selected object
                selectedObject.GetComponent<Outline>().enabled = false;
            }

            selectedObject = obj;

            if (selectedObject != null)
            {
                // Enable outline on the newly selected object
                selectedObject.GetComponent<Outline>().enabled = true;
            }
        }
    }
    public void DeselectObject()
    {
        if (replacer.insertON == false && replacer.replaceON == false)
        {
            if (selectedObject != null)
            {
                // Disable outline on the previously selected object
                selectedObject.GetComponent<Outline>().enabled = false;
            }

            selectedObject = null;

            
        }
    }
    public void RotateLeft()
    {
        Debug.Log("Left Button");
        if (isObjectSelected && !selectedObject.name.StartsWith("Straight"))
        {
            // Rotate the selected object 15 degrees to the left
            selectedObject.transform.Rotate(Vector3.up, -rotationAngle, Space.Self);

            // Check if the selected object has a child with name starting "Straight"
            Transform snapPoint = selectedObject.transform.Find("SnapPoint");
            if (snapPoint != null)
            {
                for (int i = 0; i < snapPoint.childCount; i++)
                {
                    Transform child = snapPoint.GetChild(i);
                    if (child.name.StartsWith("Straight"))
                    {
                        // Rotate the straight child object around the Y-axis opposite to the selected object
                        child.Rotate(Vector3.up, rotationAngle, Space.Self);
                        break;
                    }
                }
            }
        }
        else if (selectedObject.name.StartsWith("Straight"))
        {
            Transform snapPoint = selectedObject.transform.Find("SnapPoint");
            if (snapPoint != null)
            {
                Transform curvedChild = null;

                // Find the first child with a name starting with "Curved" in the snapPoint children
                foreach (Transform child in snapPoint)
                {
                    if (child.name.StartsWith("Curved"))
                    {
                        curvedChild = child;
                        break;
                    }
                }

                // Rotate the "Curved" child if found
                if (curvedChild != null)
                {
                    curvedChild.Rotate(Vector3.up, -rotationAngle, Space.Self);
                }
            }
        }
        startPartController.SaveData();
        hierachySize.CalculateBounds();
        indicator.MoveToLastSnapPoint();
    }
    public void RotateRight()
    {
        Debug.Log("Right Button");
        if (isObjectSelected && !selectedObject.name.StartsWith("Straight"))
        {
            // Rotate the selected object 15 degrees to the right
            selectedObject.transform.Rotate(Vector3.up, rotationAngle, Space.Self);

            // Check if the selected object has a child with name starting "Straight"
            Transform snapPoint = selectedObject.transform.Find("SnapPoint");
            if (snapPoint != null)
            {
                for (int i = 0; i < snapPoint.childCount; i++)
                {
                    Transform child = snapPoint.GetChild(i);
                    if (child.name.StartsWith("Straight"))
                    {
                        // Rotate the straight child object around the Y-axis along with the selected object
                        child.Rotate(Vector3.up, -rotationAngle, Space.Self);
                        break;
                    }
                }
            }
        }
        else if (selectedObject.name.StartsWith("Straight"))
        {
            Transform snapPoint = selectedObject.transform.Find("SnapPoint");
            if (snapPoint != null)
            {
                Transform curvedChild = null;

                // Find the first child with a name starting with "Curved" in the snapPoint children
                foreach (Transform child in snapPoint)
                {
                    if (child.name.StartsWith("Curved"))
                    {
                        curvedChild = child;
                        break;
                    }
                }

                // Rotate the "Curved" child if found
                if (curvedChild != null)
                {
                    curvedChild.Rotate(Vector3.up, rotationAngle, Space.Self);
                }
            }
        }
        startPartController.SaveData();
        hierachySize.CalculateBounds();
        indicator.MoveToLastSnapPoint();
    }
    public void SingleRotateLeft()
    {
        if (isObjectSelected && !selectedObject.name.StartsWith("Straight"))
        {
            selectedObject.transform.Rotate(Vector3.up, -rotationAngle, Space.Self);
            RotateChildObject(rotationAngle);
        }
    }

    public void SingleRotateRight()
    {
        if (isObjectSelected && !selectedObject.name.StartsWith("Straight"))
        {
            selectedObject.transform.Rotate(Vector3.up, rotationAngle, Space.Self);
            RotateChildObject(-rotationAngle);
        }
    }

    private void RotateChildObject(float angle)
    {
        Transform snapPoint = selectedObject.transform.Find("SnapPoint");
        if (snapPoint != null)
        {
            for (int i = 0; i < snapPoint.childCount; i++)
            {
                Transform child = snapPoint.GetChild(i);
                if (child.name.StartsWith("Straight"))
                {
                    //child.Rotate(Vector3.up, angle, Space.Self);
                    break;
                }
            }
        }
    }

    public void SetRotateLeft(bool value)
    {
        if (value)
        {
            rotateLeft = true;
            rotateRight = false;
            StopRotationCoroutine();
            rotationCoroutine = StartCoroutine(ContinuousRotation());
        }
        else
        {
            rotateLeft = false;
            StopRotationCoroutine();
        }
    }

    public void SetRotateRight(bool value)
    {
        if (value)
        {
            rotateRight = true;
            rotateLeft = false;
            StopRotationCoroutine();
            rotationCoroutine = StartCoroutine(ContinuousRotation());
        }
        else
        {
            rotateRight = false;
            StopRotationCoroutine();
        }
    }

    private void StopRotationCoroutine()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }

    private IEnumerator ContinuousRotation()
    {
        while (true)
        {
            if (rotateLeft)
            {
                RotateLeft();
            } 
            else if (rotateRight)
            {
                RotateRight();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ChangeColorToRed()
    {
        if (selectedObject != null)
        {
            if (bodyColorOn == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = BodyMaterials[0];
                        }
                    }
                }
            }

            if (nutsColorON == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name == "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = NutsMaterials[0];
                        }

                        foreach (Transform grandChild in child.transform)
                        {
                            if (grandChild.name == "SnapPoint_nuts")
                            {
                                Renderer snapRenderer = grandChild.GetComponent<Renderer>();

                                if (snapRenderer != null)
                                {
                                    snapRenderer.sharedMaterial = NutsMaterials[0];
                                }
                            }
                        }
                    }
                }
            }

            if (windowsColorOn && windowsToggle)
            {
                // Disable the outline while changing materials
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = false;
                }

                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts" && child.name != "SnapPoint_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            Debug.Log(WindowMaterials[4]);
                            Material[] materials = renderer.sharedMaterials;
                            for (int i = 0; i < materials.Length; i++)
                            {
                                if (materials[i].name == "WindowYellow" || materials[i].name == "WindowRed" || materials[i].name == "WindowGrey" || materials[i].name == "WindowGreen" || materials[i].name == "WindowBlue")
                                {
                                    materials[i] = WindowMaterials[0]; // Grey window material
                                }
                            }
                            // Set the modified materials back to the renderer
                            renderer.sharedMaterials = materials;
                        }
                    }
                }

                // Re-enable the outline if this is the selected object
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
            }
        }

        startPartController.SaveData();
    }

    public void ChangeColorToBlue()
    {
        if (selectedObject != null)
        {
            if (bodyColorOn == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = BodyMaterials[1];
                        }
                    }
                }
            }

            if (nutsColorON == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name == "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = NutsMaterials[1];
                        }

                        foreach (Transform grandChild in child.transform)
                        {
                            if (grandChild.name == "SnapPoint_nuts")
                            {
                                Renderer snapRenderer = grandChild.GetComponent<Renderer>();

                                if (snapRenderer != null)
                                {
                                    snapRenderer.sharedMaterial = NutsMaterials[1];
                                }
                            }
                        }
                    }
                }
            }

            if (windowsColorOn && windowsToggle)
            {
                // Disable the outline while changing materials
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = false;
                }

                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts" && child.name != "SnapPoint_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            Debug.Log(WindowMaterials[4]);
                            Material[] materials = renderer.sharedMaterials;
                            for (int i = 0; i < materials.Length; i++)
                            {
                                if (materials[i].name == "WindowYellow" || materials[i].name == "WindowRed" || materials[i].name == "WindowGrey" || materials[i].name == "WindowGreen" || materials[i].name == "WindowBlue")
                                {
                                    materials[i] = WindowMaterials[1]; // Grey window material
                                }
                            }
                            // Set the modified materials back to the renderer
                            renderer.sharedMaterials = materials;
                        }
                    }
                }

                // Re-enable the outline if this is the selected object
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
            }
        }

        startPartController.SaveData();
    }

    public void ChangeColorToYellow()
    {
        if (selectedObject != null)
        {
            if (bodyColorOn == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = BodyMaterials[2];
                        }
                    }
                }
            }

            if (nutsColorON == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name == "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = NutsMaterials[2];
                        }

                        foreach (Transform grandChild in child.transform)
                        {
                            if (grandChild.name == "SnapPoint_nuts")
                            {
                                Renderer snapRenderer = grandChild.GetComponent<Renderer>();

                                if (snapRenderer != null)
                                {
                                    snapRenderer.sharedMaterial = NutsMaterials[2];
                                }
                            }
                        }
                    }
                }
            }

            if (windowsColorOn && windowsToggle)
            {
                // Disable the outline while changing materials
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = false;
                }

                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts" && child.name != "SnapPoint_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            Debug.Log(WindowMaterials[4]);
                            Material[] materials = renderer.sharedMaterials;
                            for (int i = 0; i < materials.Length; i++)
                            {
                                if (materials[i].name == "WindowYellow" || materials[i].name == "WindowRed" || materials[i].name == "WindowGrey" || materials[i].name == "WindowGreen" || materials[i].name == "WindowBlue")
                                {
                                    materials[i] = WindowMaterials[2]; // Grey window material
                                }
                            }
                            // Set the modified materials back to the renderer
                            renderer.sharedMaterials = materials;
                        }
                    }
                }

                // Re-enable the outline if this is the selected object
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
            }
        }

        startPartController.SaveData();
    }

    public void ChangeColorToGreen()
    {
        if (selectedObject != null)
        {
            if (bodyColorOn == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = BodyMaterials[3];
                        }
                    }
                }
            }

            if (nutsColorON == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name == "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = NutsMaterials[3];
                        }

                        foreach (Transform grandChild in child.transform)
                        {
                            if (grandChild.name == "SnapPoint_nuts")
                            {
                                Renderer snapRenderer = grandChild.GetComponent<Renderer>();

                                if (snapRenderer != null)
                                {
                                    snapRenderer.sharedMaterial = NutsMaterials[3];
                                }
                            }
                        }
                    }
                }
            }

            if (windowsColorOn && windowsToggle)
            {
                // Disable the outline while changing materials
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = false;
                }

                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts" && child.name != "SnapPoint_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            Debug.Log(WindowMaterials[4]);
                            Material[] materials = renderer.sharedMaterials;
                            for (int i = 0; i < materials.Length; i++)
                            {
                                if (materials[i].name == "WindowYellow" || materials[i].name == "WindowRed" || materials[i].name == "WindowGrey" || materials[i].name == "WindowGreen" || materials[i].name == "WindowBlue")
                                {
                                    materials[i] = WindowMaterials[3]; // Grey window material
                                }
                            }
                            // Set the modified materials back to the renderer
                            renderer.sharedMaterials = materials;
                        }
                    }
                }

                // Re-enable the outline if this is the selected object
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
            }
        }

        startPartController.SaveData();
    }

    public void ChangeColorToGrey()
    {
        if (selectedObject != null)
        {
            if (bodyColorOn == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = BodyMaterials[4];
                        }
                    }
                }
            }

            if (nutsColorON == true)
            {
                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name == "Origin_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = NutsMaterials[4];
                        }

                        foreach (Transform grandChild in child.transform)
                        {
                            if (grandChild.name == "SnapPoint_nuts")
                            {
                                Renderer snapRenderer = grandChild.GetComponent<Renderer>();

                                if (snapRenderer != null)
                                {
                                    snapRenderer.sharedMaterial = NutsMaterials[4];
                                }
                            }
                        }
                    }
                }
            }

            if (windowsColorOn && windowsToggle)
            {
                // Disable the outline while changing materials
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = false;
                }

                foreach (Transform child in selectedObject.transform)
                {
                    if (child.name != "Origin_nuts" && child.name != "SnapPoint_nuts")
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            Debug.Log(WindowMaterials[4]);
                            Material[] materials = renderer.sharedMaterials;
                            for (int i = 0; i < materials.Length; i++)
                            {
                                if (materials[i].name == "WindowYellow" || materials[i].name == "WindowRed" || materials[i].name == "WindowGrey" || materials[i].name == "WindowGreen" || materials[i].name == "WindowBlue")
                                {
                                    materials[i] = WindowMaterials[4]; // Grey window material
                                }
                            }
                            // Set the modified materials back to the renderer
                            renderer.sharedMaterials = materials;
                        }
                    }
                }

                // Re-enable the outline if this is the selected object
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
            }
        }

        startPartController.SaveData();
    }

    public void BodyButtonClick()
    {
        if(!bodyColorOn)
        {
            bodyColorOn= true;
            nutsColorON = false;
            windowsColorOn = false;
            bodyButton.SwitchColorON();
            nutsButton.SwitchColorOFF();
            windowsButton.SwitchColorOFF();

        }
    }
    public void NutsButtonClick()
    {
        if (!nutsColorON)
        {
            nutsColorON = true;
            bodyColorOn = false;
            windowsColorOn = false;
            bodyButton.SwitchColorOFF();
            nutsButton.SwitchColorON();
            windowsButton.SwitchColorOFF();
        }
    }
   
    public void WindowButtonClick()
    {
        if(!windowsColorOn)
        {
            windowsColorOn= true;
            bodyColorOn = false;
            nutsColorON = false;
            bodyButton.SwitchColorOFF();
            nutsButton.SwitchColorOFF();
            windowsButton.SwitchColorON();
        }
    }

}