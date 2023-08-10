using UnityEngine;
using System.Linq;

public class RemoveCloneSuffix : MonoBehaviour
{
    private GameObject[] slideParts;

    private void Start()
    {
        // Find all objects with tag "SlideParts"
        slideParts = GameObject.FindGameObjectsWithTag("SlideParts");

        // Remove "(Clone)" suffix from their names
        foreach (GameObject part in slideParts)
        {
            if (part.name.EndsWith("(Clone)"))
            {
                part.name = part.name.Substring(0, part.name.Length - 7);
            }
        }
    }

    private void Update()
    {
        // Check if any new objects with tag "SlideParts" have been added
        GameObject[] newParts = GameObject.FindGameObjectsWithTag("SlideParts")
                                    .Where(part => !slideParts.Contains(part))
                                    .ToArray();

        // If so, add them to the slideParts array and remove "(Clone)" suffix
        if (newParts.Length > 0)
        {
            slideParts = slideParts.Concat(newParts).ToArray();
            foreach (GameObject part in newParts)
            {
                if (part.name.EndsWith("(Clone)"))
                {
                    part.name = part.name.Substring(0, part.name.Length - 7);
                }
            }
        }
    }
}