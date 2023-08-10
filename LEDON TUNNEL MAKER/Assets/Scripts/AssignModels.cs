using UnityEngine;
using Unity.VisualScripting;

public class AssignModels : MonoBehaviour
{
    public GameObject standardModelPrefab;
    public GameObject windowModelPrefab;
    public SwitchWindow switchWindowScript;
    public ObjectRotator rotatorScript;

   

    void Start()
    {
        switchWindowScript = GameObject.Find("Replacer").GetComponent<SwitchWindow>();
        rotatorScript = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
    }

    private void Update()
    {
        if (standardModelPrefab == null && windowModelPrefab == null)
        {
            standardModelPrefab = FindChildWithTag(transform, "Standard");
            windowModelPrefab = FindChildWithTag(transform, "Window");
        }
    }

    private GameObject FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
            GameObject found = FindChildWithTag(child, tag);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}