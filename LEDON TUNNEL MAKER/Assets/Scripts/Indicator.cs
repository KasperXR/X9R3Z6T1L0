using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    private GameObject startPart;
    private GameObject lastSnapPoint;
    public GameObject cube;

    void Start()
    {

    }



    void FindLastSnapPoint(Transform parent) { 
    
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // If this child is a SnapPoint, store it as the lastSnapPoint
            if (child.name.Contains("SnapPoint"))
            {
                lastSnapPoint = child.gameObject;
            }

            // Continue searching in the children of this child
            FindLastSnapPoint(child);
        }
    }

    public void MoveToLastSnapPoint()
    {
        // Reset lastSnapPoint
        lastSnapPoint = null;
        
        // Start searching for the last SnapPoint in the StartPart hierarchy
         if(startPart != null)
        {
            FindLastSnapPoint(startPart.transform);
        }
        else
        {
            startPart = GameObject.Find("StartPart");
            FindLastSnapPoint(startPart.transform);
        }
        
        

        // If a last SnapPoint was found, move this object to its position and rotation
        if (lastSnapPoint != null)
        {
            transform.position = lastSnapPoint.transform.position;
            transform.rotation = lastSnapPoint.transform.rotation;
        }

        if (lastSnapPoint.transform.Find("Ending-End") != null)
        {

            cube.SetActive(false);
        }
        else
        {
            cube.SetActive(true);
        }
    }
}
