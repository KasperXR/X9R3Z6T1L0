using Lean.Gui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchWindow : MonoBehaviour
{
    public ObjectRotator rotatorScript;
    public GameObject selectedObject;
    public LeanToggle windowToggle;
    public GameObject windowObject;
    public GameObject standardObject;
    public AssignModels assignModelsScript;
    private bool isToggled = false;
    public StartPartController startPartController;

    void Start()
    {
        startPartController = GameObject.Find("CommandManager").GetComponent<StartPartController>();
        rotatorScript = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
        windowToggle = GameObject.Find("WindowToggle").GetComponent<LeanToggle>();

  
        
    }

    void Update()
    {

        if (rotatorScript != null && rotatorScript.selectedObject != null)
        {
            selectedObject = rotatorScript.selectedObject;
            assignModelsScript = selectedObject.GetComponent<AssignModels>();
            
        }
        windowObject = assignModelsScript.windowModelPrefab;
        standardObject = assignModelsScript.standardModelPrefab;
        if (!windowObject.active)
        {
            windowToggle.On = false;
        }
        else
        {
            windowToggle.On = true;
        }
    }

   

    public void ToggleON()
    {
        windowObject.SetActive(true);
        standardObject.SetActive(false);
        startPartController.SaveData();
    }

    public void ToggleOFF()
    {
        windowObject.SetActive(false);
        standardObject.SetActive(true);
        startPartController.SaveData();
    }

    
}