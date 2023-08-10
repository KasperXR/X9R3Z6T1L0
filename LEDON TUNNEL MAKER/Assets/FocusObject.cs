using UnityEngine;

public class FocusObject : MonoBehaviour
{
    public GameObject target;  // Your target object
    public float distance = 5f;  // Distance from the object
    public float padding = 0.5f;  // Padding around the object

    void Update()
    {
        if (target == null)
        {
            target = GameObject.Find("StartPart");
        };

        // Calculate the bounds of the object
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        for (int i = 1, len = renderers.Length; i < len; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        // Adjust the camera's position
        Vector3 pos = bounds.center;
        pos -= transform.forward * distance;
        transform.position = pos;

        // Adjust the camera's field of view or orthographic size
        if (GetComponent<Camera>().orthographic)
        {
            GetComponent<Camera>().orthographicSize = bounds.extents.magnitude + padding;
        }
        else
        {
            float objectSize = bounds.size.magnitude;
            float cameraSize = objectSize / (2f * Mathf.Tan(0.5f * GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad));
            Vector3 direction = (bounds.center - transform.position).normalized;
            transform.position = bounds.center - direction * (cameraSize + padding);
        }
    }
}
