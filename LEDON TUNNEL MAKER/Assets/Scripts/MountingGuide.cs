using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdater : MonoBehaviour
{
    // Assign these references in the Inspector
    public List<GameObject> UIObjects; // Assign your empty UI objects in this list
    public Transform SlideParent; // Assign the parent of the slide parts here

    private List<Transform> slideParts = new List<Transform>();

    private void Start()
    {
        // Gather slide parts
        foreach (Transform child in SlideParent)
        {
            if (child.gameObject.activeSelf)
            {
                slideParts.Add(child);
            }
        }

        // Update UI
        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < UIObjects.Count; i++)
        {
            // Get the corresponding Text components
            Text sizeText = UIObjects[i].transform.Find("Size").GetComponent<Text>();
            Text originNutText = UIObjects[i].transform.Find("OriginNut").GetComponent<Text>();
            Text snapPointNutText = UIObjects[i].transform.Find("SnapPointNut").GetComponent<Text>();

            if (i < slideParts.Count)
            {
                // Update the text based on the slide part's properties
                Transform slidePart = slideParts[i];
                string name = slidePart.name;

                if (name.StartsWith("straight"))
                {
                    string size = name.Split('-')[1];
                    sizeText.text = size + "cm";
                }
                else if (name.StartsWith("curved"))
                {
                    string degrees = name.Split('-')[1];
                    sizeText.text = degrees + " degrees";
                }

                // Add code to determine OriginNut and SnapPointNut values based on the rotation or any other factor
                originNutText.text = DetermineOriginNut(slidePart);
                snapPointNutText.text = DetermineSnapPointNut(slidePart);
            }
            else
            {
                // If there are no corresponding slide parts, disable the UI object
                UIObjects[i].SetActive(false);
            }
        }
    }

    // Add your logic for determining OriginNut here
    private string DetermineOriginNut(Transform slidePart)
    {
        // For now, this just returns "A" as a placeholder
        return "A";
    }

    // Add your logic for determining SnapPointNut here
    private string DetermineSnapPointNut(Transform slidePart)
    {
        // For now, this just returns "A" as a placeholder
        return "A";
    }
}
