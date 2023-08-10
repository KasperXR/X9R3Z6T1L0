using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DisableEnableWindow : MonoBehaviour
{
    private ObjectRotator rotatorScript;
    public GameObject windowToggle;
    public GameObject windowColorIcon;
    public bool hasStandardChild = false;
    // Start is called before the first frame update
    void Start()
    {
        rotatorScript = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
    }

    // Update is called once per frame
    void Update()
    {
        {
            if (rotatorScript.selectedObject != null) // Check if selectedObject is not null
            {
                if (rotatorScript.selectedObject.name.StartsWith("Curved") || rotatorScript.selectedObject.name == "StartPart" || rotatorScript.selectedObject.name.StartsWith("Pole") || rotatorScript.selectedObject.name.StartsWith("Ending") || rotatorScript.selectedObject.name.StartsWith("Straight-35"))
                {
                    windowToggle.SetActive(false);
                }
                else
                {
                    windowToggle.SetActive(true);
                }
            }
        }
        
        foreach (Transform child in rotatorScript.selectedObject.transform)
        {
            if (child.gameObject.activeSelf && child.gameObject.tag == "Standard")
            {
                hasStandardChild = true;
                 // If we found a child with "Standard" tag, no need to continue the loop
            }
            else if(child.gameObject.activeSelf && child.gameObject.tag == "Window"){
                hasStandardChild = false;
            }
        }
        if (rotatorScript.selectedObject.tag == "Curved" || hasStandardChild) { 
        windowColorIcon.SetActive(false);
            //rotatorScript.BodyButtonClick();
        }
        else
        {
            windowColorIcon.SetActive(true);
        }
    }
    
}

