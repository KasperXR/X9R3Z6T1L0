using UnityEngine;

public class HeightIndicatorController : MonoBehaviour
{
    public string targetObjectName = "StartPart";
    public float maxHeight = 1.0f;

    private Transform targetObject;
    private Transform cylinderTransform;

    void Start()
    {
        targetObject = GameObject.Find(targetObjectName).transform;
        if (targetObject == null)
        {
            Debug.LogError("No object found with the name: " + targetObjectName);
            enabled = false;
            return;
        }

        cylinderTransform = transform.GetChild(0);
    }

    void Update()
    {
        float targetHeight = Mathf.Clamp(targetObject.localScale.y, 0, maxHeight);
        cylinderTransform.localScale = new Vector3(cylinderTransform.localScale.x, targetHeight, cylinderTransform.localScale.z);
        cylinderTransform.position = new Vector3(targetObject.position.x, targetHeight / 2, targetObject.position.z);
    }
}