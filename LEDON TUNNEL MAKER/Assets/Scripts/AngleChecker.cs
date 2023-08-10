using UnityEngine;

public class ObjectAngleChecker : MonoBehaviour
{
    public float angleThreshold = 5.0f; // The angle threshold

    void Update()
    {
        CheckObjectAngle();
    }

    void CheckObjectAngle()
    {
        // Calculate dot product
        float dotProduct = Vector3.Dot(transform.forward, Vector3.up);

        // Convert dot product to an angle (in degrees)
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        if (Mathf.Abs(angle - 90) > angleThreshold) // If angle deviates more than the threshold from 90
        {
            Debug.Log(gameObject.name + " is tilted by more than " + angleThreshold + " degrees. Current angle: " + angle);
        }
    }
}
