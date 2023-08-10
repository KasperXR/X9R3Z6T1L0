using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class HierarchyBounds : MonoBehaviour
{
    public TMP_Text lengthText;
    public TMP_Text widthText;
    private Vector3 lastSize;
    public GameObject targetObject;
    public GameObject boundsCube;
    public TMP_Text boundsAssemblyText;
    public TMP_Text heightAssembltText;




    private void FixedUpdate()
    {
        if(targetObject == null) {

            targetObject = GameObject.Find("StartPart");
        
        }
    }

    public void CalculateBounds()
    {
        Bounds bounds = GetHierarchyBounds();
        Vector3 size = bounds.size;

        lengthText.text = (size.z*100).ToString("F0") + "cm";
        widthText.text = (size.x*100).ToString("F0") + "cm";
        boundsAssemblyText.text = "Area: " + (size.z * 100).ToString("F0") + "cm"+ " x " + (size.x * 100).ToString("F0") + "cm";
        heightAssembltText.text = "Start Height: "+((targetObject.transform.position.y - 0.3805935f)*100).ToString("F0")+ "cm";
        // Position text objects at the ends of the bounds
        lengthText.transform.position = new Vector3(bounds.min.x, 0, bounds.center.z);
        widthText.transform.position = new Vector3(bounds.center.x, 0, bounds.min.z);

        // Scale and position the cube to match the bounds
        boundsCube.transform.position = new Vector3(bounds.center.x, 0.02f, bounds.center.z);
        boundsCube.transform.localScale = new Vector3(bounds.size.x, 0.001f, bounds.size.z);

        //Debug.Log("Width: " + size.x + "\nLength: " + size.z);
    }

    private Bounds GetHierarchyBounds()
    {
        Bounds bounds = new Bounds();
        bool hasBounds = false;

        // Get all MeshFilter components from children where gameObject.name starts with "model"
        foreach (MeshFilter mf in targetObject.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.gameObject.name.StartsWith("model"))
            {
                // Skip if no valid mesh is present
                if (mf.sharedMesh == null) continue;

                // Get mesh vertices
                Vector3[] vertices = mf.sharedMesh.vertices;

                // Iterate over all vertices
                for (int i = 0; i < vertices.Length; i++)
                {
                    // Transform vertex to world space
                    Vector3 v = mf.transform.TransformPoint(vertices[i]);

                    if (hasBounds)
                    {
                        bounds.Encapsulate(v); // Extend the bounds to include the vertex
                    }
                    else
                    {
                        bounds = new Bounds(v, Vector3.zero); // Initialize the bounds to the vertex
                        hasBounds = true;
                    }
                }
            }
        }

        Debug.Log("Bounds size: " + bounds.size);

        return bounds;
    }
}
