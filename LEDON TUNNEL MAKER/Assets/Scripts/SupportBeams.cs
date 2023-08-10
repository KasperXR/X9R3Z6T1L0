using Lean.Gui;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static AnnotationScript;

public class SupportBeams : MonoBehaviour
{
    public GameObject cylinderPrefab;
    public float maxDistance = 10f;
    private ObjectRotator objectRotatorScript;
    private RaycastHit hit;
    private RaycastHit closestHit;
    private AnnotationScript annotation;
    private bool middleBeamDetected = false;

    private void Start()
    {
        // Find and assign the ObjectRotator script to the objectRotatorScript variable
        objectRotatorScript = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
        annotation = GameObject.Find("Annotations").GetComponent<AnnotationScript>();
    }

    Vector3 GetLowestPoint(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        Bounds bounds = collider.bounds;
        Vector3 lowestPoint = bounds.center - new Vector3(0, bounds.extents.y, 0);

        return lowestPoint;
    }
    Vector3 GetHighestPoint(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        Bounds bounds = collider.bounds;
        Vector3 highestPoint = bounds.center + new Vector3(0, bounds.extents.y, 0);

        return highestPoint;
    }
    private GameObject GetOppositeConnectionPoint(GameObject selectedObject)
    {
        GameObject connectionPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        connectionPoint.name = "OppositeConnectionPoint";
        connectionPoint.transform.SetParent(selectedObject.transform);

        // Calculate the angle with the highest point on the object
        Vector3 highestPointLocal = selectedObject.transform.InverseTransformPoint(GetHighestPoint(selectedObject));
        float angle = Mathf.Atan2(highestPointLocal.z, highestPointLocal.x);
        // Convert the angle to degrees and round to the nearest 15 degrees
        float angleDegrees = Mathf.Round((Mathf.Rad2Deg * angle) / 15f) * 15f;
        // Convert the angle back to radians
        float newAngle = Mathf.Deg2Rad * angleDegrees;

        // Calculate the connection point's local position
        Vector3 localConnectionPos = new Vector3(0.4f * Mathf.Cos(newAngle), 0, 0.4f * Mathf.Sin(newAngle));

        // Set the connection point's local position
        connectionPoint.transform.localPosition = localConnectionPos;

        connectionPoint.transform.localRotation = Quaternion.identity;
        connectionPoint.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        // Remove the collider from the connection point sphere
        Destroy(connectionPoint.GetComponent<Collider>());

        return connectionPoint;
    }


    private GameObject CreateConnectionPoint(GameObject selectedObject, float angleOffset = 0)
    {
        GameObject connectionPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        connectionPoint.name = "ConnectionPoint";
        connectionPoint.transform.SetParent(selectedObject.transform);

        // Calculate the angle with the lowest point on the object and add the offset
        float angle = Mathf.Atan2(selectedObject.transform.InverseTransformPoint(GetLowestPoint(selectedObject)).z, selectedObject.transform.InverseTransformPoint(GetLowestPoint(selectedObject)).x) + angleOffset;

        // Calculate the connection point's local position
        Vector3 localConnectionPos = new Vector3(0.4f * Mathf.Cos(angle), 0, 0.4f * Mathf.Sin(angle));

        // Set the connection point's local position
        connectionPoint.transform.localPosition = localConnectionPos;

        connectionPoint.transform.localRotation = Quaternion.identity;
        connectionPoint.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        // Remove the collider from the connection point sphere
        Destroy(connectionPoint.GetComponent<Collider>());

        return connectionPoint;
    }

    public void CreateSupportBeams()
    {
        // Check if an object is currently selected
        if (objectRotatorScript.selectedObject != null)
        {
            Transform childToDestroy = null;
            // Iterate through all direct children of the selected object
            for (int i = 0; i < objectRotatorScript.selectedObject.transform.childCount; i++)
            {
                Transform child = objectRotatorScript.selectedObject.transform.GetChild(i);

                // Check if the child's name starts with "SupportBeam"
                if (child.name.StartsWith("SupportBeam"))
                {
                    childToDestroy = child;
                    break; // exit the loop once we found the match
                }
            }

            // Check if a matching child was found
            if (childToDestroy != null)
            {
                // Destroy the child object
                Destroy(childToDestroy.gameObject);
            }
            // Define the start position of the support beams
            GameObject connectionPointLeft = CreateConnectionPoint(objectRotatorScript.selectedObject, -15f * Mathf.Deg2Rad);
            GameObject connectionPointRight = CreateConnectionPoint(objectRotatorScript.selectedObject, 15f * Mathf.Deg2Rad);
            Vector3 startLeft = connectionPointLeft.transform.position;
            Vector3 startRight = connectionPointRight.transform.position;

            // Define the layer mask for raycasting to include only the "SnappedObjects" layer
            int snappedObjectsLayerMask = 1 << LayerMask.NameToLayer("SnappedObjects");

            // Define the layer mask for raycasting to exclude the "SnappedObjects" layer
            int otherObjectsLayerMask = ~(1 << LayerMask.NameToLayer("SnappedObjects"));

            // Temporarily disable the collider of the selected object to prevent the raycast from hitting it
            Collider selectedObjectCollider = objectRotatorScript.selectedObject.GetComponent<Collider>();
            Collider parentParentObjectCollider = objectRotatorScript.selectedObject.transform.parent?.parent?.GetComponent<Collider>();
            if (selectedObjectCollider != null) selectedObjectCollider.enabled = false;
            if (parentParentObjectCollider != null) parentParentObjectCollider.enabled = false;

            // Create an empty GameObject as a parent for support beams
            GameObject parentObject = new GameObject("SupportBeam-double");
            parentObject.tag = "Beam";
            parentObject.transform.position = (startLeft + startRight) / 2;

            // Create the two support beams at -15 and +15 degrees
            CreateSupportBeamWithAngle(startLeft, 15f, snappedObjectsLayerMask, otherObjectsLayerMask, parentObject, connectionPointLeft, true);
            CreateSupportBeamWithAngle(startRight, -15f, snappedObjectsLayerMask, otherObjectsLayerMask, parentObject, connectionPointRight, true);

            // Re-enable the collider of the selected object
            if (selectedObjectCollider != null) selectedObjectCollider.enabled = true;
            if (parentParentObjectCollider != null) parentParentObjectCollider.enabled = true;


            // Destroy the connection points after the support beams are created
            Destroy(connectionPointLeft); //Updated this line
            Destroy(connectionPointRight); //Updated this line

            Vector3 parentRotation = objectRotatorScript.selectedObject.transform.eulerAngles;

            // Rotate the parent GameObject
            //parentObject.transform.Rotate(0, parentRotation.y - 90f, 0);
            parentObject.transform.SetParent(objectRotatorScript.selectedObject.transform);
            parentObject.transform.SetSiblingIndex(2);
            float beamLength = (parentObject.transform.GetChild(0).transform.localScale.y * 2);
            if (beamLength < 0.66)
            {
                parentObject.name = "SupportBeam-double-66";
            }
            else if (beamLength > 0.66 && beamLength <= 0.9)
            {
                parentObject.name = "SupportBeam-double-90";
            }
            else if (beamLength > 0.9 && beamLength <= 1.27)
            {
                parentObject.name = "SupportBeam-double-127";
            }
            else if (beamLength > 1.27 && beamLength <= 1.63)
            {
                parentObject.name = "SupportBeam-double-163";
            }
            else if (beamLength > 1.63 && beamLength <= 2.20)
            {
                parentObject.name = "SupportBeam-double-220";
            }
            else if (beamLength > 2.20 && beamLength <= 2.80)
            {
                parentObject.name = "SupportBeam-double-280";
            }
            else if (beamLength > 2.80 && beamLength <= 3.20)
            {
                parentObject.name = "SupportBeam-double-320";
            }
            else if (beamLength > 3.20 && beamLength <= 4.20)
            {
                parentObject.name = "SupportBeam-double-420";
            }
            else if (beamLength > 4.20)
            {
                parentObject.name = "SupportBeam-Unsupported";
                annotation.CreateAnnotation(AnnotationType.Warning, "There is a double support beam longer than 420cm which is cannot be provided by LEDON.", "CirclePlane");
            }
        }
    }


    private void CreateSupportBeamWithAngle(Vector3 start, float angle, int snappedObjectsLayerMask, int otherObjectsLayerMask, GameObject parentObject, GameObject connectionPoint, bool useOppositeConnectionPoint = false)
    {
        // Define the direction of the support beam as always parallel to the ground
        Vector3 direction = Quaternion.AngleAxis(angle, objectRotatorScript.selectedObject.transform.up) * Vector3.down;

        Vector3 end = start + direction * maxDistance;


        // Check if there is a slide part below the selected object
        if (Physics.Raycast(start, direction, out hit, maxDistance, snappedObjectsLayerMask))
        {
            // Get the highest point of the object below
            GameObject hitObject = hit.collider.gameObject;

            if (useOppositeConnectionPoint)
            {
                GameObject oppositeConnectionPoint = GetOppositeConnectionPoint(hitObject);
                end = oppositeConnectionPoint.transform.position;
                Debug.DrawLine(start, hit.point, Color.red, 50.0f);
                // Destroy the opposite connectionPoint after it's used
                Destroy(oppositeConnectionPoint);
            }
        }
        // If there's no slide part below the selected object, check if there is another object below
        else if (Physics.Raycast(start, direction, out hit, maxDistance, otherObjectsLayerMask))
        {
            end = hit.point;
            Debug.DrawLine(start, hit.point, Color.red, 50.0f);
        }

        // Call the original CreateSupportBeam method to create a single support beam
        CreateSupportBeam(start, end, parentObject);
    }

    public void CreateSupportBeam(Vector3 start, Vector3 end, GameObject parentObject)
    {
        // Calculate the distance and scale of the support beam
        float distance = Vector3.Distance(start, end);
        Vector3 scale = new Vector3(0.1f, distance / 2f, 0.1f);

        // Check if the distance is greater than a small threshold, and create the support beam if so
        if (distance > 0.01f)
        {
            GameObject cylinder = GameObject.Instantiate(cylinderPrefab);
            Vector3 position = Vector3.Lerp(start, end, 0.5f);

            cylinder.transform.position = position;

            // Orient the support beam along the direction from start to end
            cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);

            // Set a minimum scale to ensure the cylinder is visible
            float minScale = 0.01f;
            cylinder.transform.localScale = Vector3.Max(scale, new Vector3(minScale, minScale, minScale));
            cylinder.transform.parent = parentObject.transform;

            GameObject endPoint = new GameObject("EndPoint");
            endPoint.transform.parent = cylinder.transform;
            endPoint.transform.position = end;

            GameObject startPoint = new GameObject("StartPoint");
            startPoint.transform.parent = cylinder.transform;
            startPoint.transform.position = start;
        }
    }
    public void CreateSingleSupportBeam()
    {
        // Check if an object is currently selected
        if (objectRotatorScript.selectedObject != null)
        {
            Transform childToDestroy = null;
            // Iterate through all direct children of the selected object
            for (int i = 0; i < objectRotatorScript.selectedObject.transform.childCount; i++)
            {
                Transform child = objectRotatorScript.selectedObject.transform.GetChild(i);

                // Check if the child's name starts with "SupportBeam"
                if (child.name.StartsWith("SupportBeam"))
                {
                    childToDestroy = child;
                    break; // exit the loop once we found the match
                }
            }

            // Check if a matching child was found
            if (childToDestroy != null)
            {
                // Destroy the child object
                Destroy(childToDestroy.gameObject);
            }

            // Define the start and end positions of the support beam
            GameObject connectionPoint = CreateConnectionPoint(objectRotatorScript.selectedObject);
            Vector3 start = connectionPoint.transform.position;
            Vector3 end = start - objectRotatorScript.selectedObject.transform.up * maxDistance;

            // Define the layer mask for raycasting to include only the "SnappedObjects" layer
            int snappedObjectsLayerMask = 1 << LayerMask.NameToLayer("SnappedObjects");

            // Define the layer mask for raycasting to exclude the "SnappedObjects" layer
            int otherObjectsLayerMask = ~(1 << LayerMask.NameToLayer("SnappedObjects"));

            // Temporarily disable the collider of the selected object to prevent the raycast from hitting it
            Collider selectedObjectCollider = objectRotatorScript.selectedObject.GetComponent<Collider>();
            Collider parentParentObjectCollider = objectRotatorScript.selectedObject.transform.parent?.parent?.GetComponent<Collider>();
            if (selectedObjectCollider != null) selectedObjectCollider.enabled = false;
            if (parentParentObjectCollider != null) parentParentObjectCollider.enabled = false;

            // Create an empty GameObject as a parent for support beams
            GameObject parentObject = new GameObject("SupportBeam-single");
            parentObject.tag = "Beam";
            parentObject.transform.position = start;

            // Check if there is a slide part below the selected object
            if (Physics.Raycast(start, -Vector3.up, out hit, maxDistance, snappedObjectsLayerMask))
            {
                // Get the highest point of the object below
                GameObject hitObject = hit.collider.gameObject;
                GameObject oppositeConnectionPoint = GetOppositeConnectionPoint(hitObject);
                end = oppositeConnectionPoint.transform.position;
                middleBeamDetected = true;
                // Destroy the opposite connection point after it's used
                Destroy(oppositeConnectionPoint);
            }
            // If there's no slide part below the selected object, check if there is another object below
            else if (Physics.Raycast(start, -Vector3.up, out hit, maxDistance, otherObjectsLayerMask))
            {

                end = hit.point;
                middleBeamDetected = false;
            }

            // Re-enable the collider of the selected object
            if (selectedObjectCollider != null) selectedObjectCollider.enabled = true;
            if (parentParentObjectCollider != null) parentParentObjectCollider.enabled = true;


            // Create a single support beam
            CreateSupportBeam(start, end, parentObject);
            parentObject.transform.SetParent(objectRotatorScript.selectedObject.transform);
            parentObject.transform.SetSiblingIndex(2);
            float beamLength = parentObject.transform.GetChild(0).transform.localScale.y * 2;
            if (beamLength < 0.66)
            {
                if (middleBeamDetected)
                {
                    parentObject.name = "SupportBeam-middle-66";
                }
                else
                {
                    parentObject.name = "SupportBeam-single-66";
                }

            }
            else if (beamLength > 0.66 && beamLength <= 0.9)
            {
                if (middleBeamDetected)
                {
                    parentObject.name = "SupportBeam-middle-90";
                }
                else
                {
                    parentObject.name = "SupportBeam-single-90";
                }
            }
            else if (beamLength > 0.9 && beamLength <= 1.27)
            {
                if (middleBeamDetected)
                {
                    parentObject.name = "SupportBeam-middle-127";
                }
                else
                {
                    parentObject.name = "SupportBeam-single-127";
                }
            }
            else if (beamLength > 1.27 && beamLength <= 1.63)
            {
                if (middleBeamDetected)
                {
                    parentObject.name = "SupportBeam-middle-163";
                }
                else
                {
                    parentObject.name = "SupportBeam-single-163";
                }
            }
            else if (beamLength > 1.63 && beamLength <= 2.20)
            {
                if (middleBeamDetected)
                {
                    parentObject.name = "SupportBeam-middle-220";
                }
                else
                {
                    parentObject.name = "SupportBeam-single-220";
                }
            }
            else if (beamLength > 2.20 && beamLength <= 2.80)
            {
                if (middleBeamDetected)
                {
                    parentObject.name = "SupportBeam-middle-280";
                }
                else
                {
                    parentObject.name = "SupportBeam-single-280";
                }
            }
            else if (beamLength > 2.80 && beamLength <= 3.20)
            {
                if (middleBeamDetected)
                {
                    parentObject.name = "SupportBeam-middle-320";
                }
                else
                {
                    parentObject.name = "SupportBeam-single-320";
                }
            }
            else if (beamLength > 3.20 && beamLength <= 4.20)
            {
                if (middleBeamDetected)
                {
                    parentObject.name = "SupportBeam-middle-420";
                }
                else
                {
                    parentObject.name = "SupportBeam-single-420";
                }
            }
            else if (beamLength > 4.20)
            {
                parentObject.name = "SupportBeam-Unsupported";
                annotation.CreateAnnotation(AnnotationType.Warning, "There is a single support beam longer than 420cm which is cannot be provided by LEDON.", "CirclePlane");
            }

            // Destroy the connection point after the support beam is created
            Destroy(connectionPoint);
        }
    }

}