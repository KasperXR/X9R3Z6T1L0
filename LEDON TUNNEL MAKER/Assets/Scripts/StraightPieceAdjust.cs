using UnityEngine;

public class StraightPieceAdjust : MonoBehaviour
{
    private float rotationSpeed = 10f;
    private Color rayColor = Color.red;
    private Color shortestRayColor = Color.green;

    void Update()
    {
        // Check if the object is a child of another object
        if (transform.parent == null)
        {
            return;
        }

        float shortestDistance = float.MaxValue;
        Quaternion bestRotation = transform.rotation;

        int layerMask = 1 << LayerMask.NameToLayer("Plane");

        // Array to store the rays for visualization
        Ray[] rays = new Ray[24];

        // Loop through 24 angles, each 15 degrees apart
        for (int i = 0; i < 24; i++)
        {
            // Calculate the direction of the raycast for the current angle
            Quaternion rotation = transform.rotation * Quaternion.Euler(0, i * 15, 0);
            Vector3 direction = rotation * Vector3.right;

            Ray ray = new Ray(transform.position, direction);
            rays[i] = ray;

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                // If the raycast hit and the distance is shorter than the current shortest distance, update shortestDistance and bestRotation
                if (hit.distance < shortestDistance)
                {
                    shortestDistance = hit.distance;
                    bestRotation = rotation;
                }
            }
        }

        // Draw all the rays in red
       // foreach (Ray ray in rays)
       ///{
        //    Debug.DrawRay(ray.origin, ray.direction * 10f, rayColor);
        //}

        // Draw the ray with the shortest distance in green
        //Debug.DrawRay(rays[(int)(bestRotation.eulerAngles.y / 15)].origin, rays[(int)(bestRotation.eulerAngles.y / 15)].direction * 10f, shortestRayColor);

        // Directly set the rotation of the object to the best rotation
        transform.rotation = bestRotation;
    }
}
