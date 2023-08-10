using Lean.Gui;
using UnityEngine;

public class WindowObject : MonoBehaviour
{
    public SwitchWindow switchWindowscript;
    public ObjectRotator rotatorScript;
    public GameObject original;
    public GameObject windowReplacement;
    public GameObject selectedObject;
    public LeanToggle windowToggle;
    public LeanButton windowsButton;
    public SwitchWindow switchWindowScript;
    public bool windowON = false;
    public bool canSwitchWindows = false; // Add this line

    // Start is called before the first frame update
    void Start()
    {
        switchWindowscript = GameObject.Find("Replacer").GetComponent<SwitchWindow>();
        rotatorScript = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
        windowToggle = GameObject.Find("WindowToggle").GetComponent<LeanToggle>();
        switchWindowScript = GameObject.Find("Replacer").GetComponent<SwitchWindow>();
        windowsButton = GameObject.Find("WindowToggle").GetComponent<LeanButton>();

    }

    // Update is called once per frame
    void Update()
    {
        // Check if the selected object has changed since last frame
        if (selectedObject != rotatorScript.selectedObject)
        {
            selectedObject = rotatorScript.selectedObject;

            // Only update the window toggle if the new selected object matches this game object
            if (selectedObject != null && selectedObject.name == gameObject.name)
            {
                if (windowReplacement != null && canSwitchWindows) // Modify this line
                {
                    windowsButton.interactable = true;
                    

                    if (!windowON)
                    {
                        windowToggle.Set(false);
                    }
                    else
                    {
                        windowToggle.Set(true);
                    }
                }
            }
            else
            {
                windowToggle.Set(false);
                windowsButton.interactable = false;
                

            }
        }
    }
}