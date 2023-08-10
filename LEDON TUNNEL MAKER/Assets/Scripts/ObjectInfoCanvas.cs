using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ObjectInfoCanvas : MonoBehaviour
{
    public ObjectRotator rotatorScript;
    private GameObject prevSelectedObject;
    public GameObject selectedObject;
    public TMP_Text nameText;
    public TMP_Text rotationText;
    public TMP_Text heightText;
    public bool positionLocked = false; // Track the lock state
    private GameObject lockButton;
    public Camera mainCamera;
    public float yOffset = 1f; // Change this value according to your needs
    private RectTransform rectTransform;
    private RectTransform canvasRect;
    public GameObject guiEmpty;
    public GameObject rotationEmpty;
    public GameObject windowEmpty;
    public GameObject colorEmpty;
    public GameObject replaceButton;
    public GameObject insertButton;
    public List<GameObject> GuiButtons = new List<GameObject>();

    void Start()
    {
        rotatorScript = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
        lockButton = GameObject.Find("LockButton");
        rectTransform = GetComponent<RectTransform>();
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        selectedObject = rotatorScript.selectedObject;

       

        if (selectedObject == null)
        {
            guiEmpty.SetActive(false);
        }
        else
        {
            guiEmpty.SetActive(true);
        }
        if (selectedObject.name == "StartPart")
        {
            foreach (GameObject go in GuiButtons)
            {
                go.SetActive(false);
            }
        }
        else
        {
            foreach(GameObject go in GuiButtons)
            {
                go.SetActive(true) ;
            }
        }

        if (selectedObject != null)
        {
           
            if (selectedObject != prevSelectedObject)
            {
                prevSelectedObject = selectedObject;
            }

            if (!positionLocked)
            {
                Vector3 offsetPosition = selectedObject.transform.position + new Vector3(0, yOffset, 0);
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(offsetPosition);

                if (screenPosition.z > 0) // Only if the object is in front of the camera
                {
                    Vector2 viewportPosition = mainCamera.WorldToViewportPoint(offsetPosition);
                    rectTransform.anchoredPosition = new Vector2(
                        ((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
                        ((viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
                    );
                }
            }

            // Update the text fields here
            if (selectedObject.name.StartsWith("Straight") || selectedObject.name.StartsWith("Pole") || selectedObject.name.StartsWith("End"))
            {
                if (selectedObject.name.StartsWith("Straight"))
                {
                    nameText.text = selectedObject.name;
                    rotationText.text = "A";
                    heightText.text = (rotatorScript.selectedObject.transform.position.y*100).ToString("F0") + "cm";
                    rotationEmpty.SetActive(true);
                    windowEmpty.SetActive(true);
                    colorEmpty.SetActive(true);
                    replaceButton.SetActive(true);
                    insertButton.SetActive(true);
                    replaceButton.GetComponent<Button>().interactable = true;
                }
                else if (selectedObject.name == "Straight-35")
                {
                    nameText.text = selectedObject.name;
                    rotationText.text = "A";
                    heightText.text = (rotatorScript.selectedObject.transform.position.y * 100).ToString("F0") + "cm";
                    windowEmpty.SetActive(false);
                    replaceButton.GetComponent<Button>().interactable = true;
                }
                else if(selectedObject.name.StartsWith("Ending")){
                    nameText.text = selectedObject.name;
                    rotationText.text = "A";
                    heightText.text = (rotatorScript.selectedObject.transform.position.y * 100).ToString("F0") + "cm";
                    replaceButton.GetComponent<Button>().interactable = false;

                }
                else if (selectedObject.name.StartsWith("Pole"))
                {
                    nameText.text = selectedObject.transform.parent.name;
                    heightText.text = (rotatorScript.selectedObject.transform.parent.transform.position.y*100).ToString("F0") + "cm";
                    rotationEmpty.SetActive(false);
                    windowEmpty.SetActive(false);
                    colorEmpty.SetActive(false);
                    replaceButton.SetActive(false);
                    insertButton.SetActive(false);
                    

                }
               
                
            }
            else
            {
                nameText.text = selectedObject.name;

                float yRotation = selectedObject.transform.localRotation.eulerAngles.y;
                int rotationNumber = Mathf.RoundToInt((yRotation + 180f) / 15f) % 24;
                if (rotationNumber == 0)
                {
                    rotationNumber = 24;
                }
                rotationText.text = rotationNumber.ToString();
                heightText.text = (rotatorScript.selectedObject.transform.position.y*100).ToString("F0") + "cm";
                rotationEmpty.SetActive(true);
                windowEmpty.SetActive(true);
                colorEmpty.SetActive(true);
                replaceButton.SetActive(true);
                insertButton.SetActive(true);
                replaceButton.GetComponent<Button>().interactable = true;
            }
        }
    }



    public void ToggleLock()
    {
        if (!positionLocked)
        {
            positionLocked = true;
            lockButton.GetComponent<RawImage>().color = Color.red;
        }
        else
        {
            positionLocked = false;
            lockButton.GetComponent<RawImage>().color = Color.green;
        }
    }
}
