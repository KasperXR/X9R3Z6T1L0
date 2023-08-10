using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class RulesChecker : MonoBehaviour
{
    public GameObject startPart;
    private float totalDistance = 0.0f;
    public AnnotationScript annotationScript;
    public TMP_Text distanceText;
    private float averageSlope = 0f;
    public SpawnStart spawnStartScript;
    private Dictionary<string, bool> annotationFlags = new Dictionary<string, bool>();
    private Transform lastChild;
    private bool infoannotation = false;
    bool isFirstStraightPart = true;
    void Start()
    {
        StartCoroutine(CheckForStartPartAndCalculateDistance());
        StartCoroutine(CheckSlideLengthAndSlopeCoroutine());
    }
    public void CheckDistanceandSlope()
    {
       
    }
    public IEnumerator CheckForStartPartAndCalculateDistance()
    {
        while (true)
        {
            startPart = GameObject.Find("StartPart");

            if (startPart != null)
            {
                totalDistance = CalculateTotalDistance(startPart.transform);
                //Debug.Log("Total Distance: " + totalDistance); // debug statement

                averageSlope = CalculateAverageSlope(spawnStartScript.existingPrefab.transform);
                //Debug.Log("Average Slope: " + averageSlope); // debug statement
                lastChild = GetLastChild(spawnStartScript.existingPrefab.transform);
                distanceText.text = "Length: " + (totalDistance*100).ToString("F0") + "cm" + "  " + "Avg slope: " + averageSlope.ToString("F0") + "°";
            }
            else
            {
                //Debug.Log("StartPart object not found"); // debug statement
            }

            yield return new WaitForSeconds(3f);
        }
    }

    // Add this to your class
    private Dictionary<int, float> rotationToDistanceMapCurved45 = new Dictionary<int, float>
{
    {-180, 73f},
    {-165, 72f},
    {-150, 69f},
    {-135, 65f},
    {-120, 59f},
    {-105, 52f},
    {-90, 44f},
    {-75, 37f},
    {-60, 30f},
    {-45, 24f},
    {-30, 20f},
    {-15, 17f},
    {0, 16f},
    {15, 17f},
    {30, 20f},
    {45, 24f},
    {60, 30f},
    {75, 37f},
    {90, 44f},
    {105, 52f},
    {120, 59f},
    {135, 65f},
    {150, 69f},
    {165, 72f}
};
    private Dictionary<int, float> rotationToDistanceMapCurved32 = new Dictionary<int, float>
{
    {-180, 53f},
    {-165, 52f},
    {-150, 50f},
    {-135, 47f},
    {-120, 42f},
    {-105, 37f},
    {-90, 32f},
    {-75, 27f},
    {-60, 22f},
    {-45, 17f},
    {-30, 14f},
    {-15, 12f},
    {0, 11f},
    {15, 12f},
    {30, 14f},
    {45, 17f},
    {60, 22f},
    {75, 27f},
    {90, 32f},
    {105, 37f},
    {120, 42f},
    {135, 47f},
    {150, 50f},
    {165, 52f}
};

    private Dictionary<int, float> rotationToDistanceMapCurved38 = new Dictionary<int, float>
{
    {-180, 62f},
    {-165, 61f},
    {-150, 59f},
    {-135, 55f},
    {-120, 50f},
    {-105, 44f},
    {-90, 38f},
    {-75, 31f},
    {-60, 26f},
    {-45, 21f},
    {-30, 17f},
    {-15, 14f},
    {0, 13f},
    {15, 14f},
    {30, 17f},
    {45, 21f},
    {60, 26f},
    {75, 31f},
    {90, 38f},
    {105, 44f},
    {120, 50f},
    {135, 55f},
    {150, 59f},
    {165, 61f}
};

    // Update CalculateTotalDistance function

    float CalculateTotalDistance(Transform currentTransform, bool isStartPart = false, int parentRotation = 0, int lastCurvedRotation = 0)
    {
        float totalDistance = 0.0f;

        int thisRotation = Mathf.RoundToInt(currentTransform.localRotation.eulerAngles.y);
        thisRotation = ((thisRotation + 180) % 360) - 180; // Ensure rotation is between -180 and 180
        thisRotation = (thisRotation / 15) * 15; // Round rotation to nearest 15 degrees

        if (currentTransform.name == "StartPart")
        {
            // Reset the flag each time you identify a StartPart
            isFirstStraightPart = true;
            isStartPart = true;
            lastCurvedRotation = 0;
        }
        else if (currentTransform.name.Contains("Straight") || currentTransform.name.Contains("Curved"))
        {
            string[] nameParts = currentTransform.name.Split('-');
            if (nameParts.Length == 2)
            {
                if (int.TryParse(nameParts[1], out int partDistanceNum))
                {
                    float partDistance = partDistanceNum / 100.0f; // Convert from cm to meters

                    // If it's a curved part, get the distance from the rotation to distance map
                    if (currentTransform.name.Contains("Curved"))
                    {
                        if (thisRotation == 0)
                        {
                            parentRotation = lastCurvedRotation;
                        }
                        else
                        {
                            parentRotation += thisRotation;
                            lastCurvedRotation = parentRotation;
                        }

                        Dictionary<int, float> rotationToDistanceMap = null;
                        switch (nameParts[1])
                        {
                            case "45":
                                rotationToDistanceMap = rotationToDistanceMapCurved45;
                                break;
                            case "32":
                                rotationToDistanceMap = rotationToDistanceMapCurved32;
                                break;
                            case "38":
                                rotationToDistanceMap = rotationToDistanceMapCurved38;
                                break;
                            default:
                                Debug.LogWarning($"Unexpected object name {currentTransform.name}");
                                return totalDistance; // Skip this object
                        }

                        if (rotationToDistanceMap != null && rotationToDistanceMap.TryGetValue(parentRotation, out float rotationDistance))
                        {
                            partDistance = rotationDistance / 100.0f; // Convert from cm to meters
                        }
                        else
                        {
                            Debug.LogWarning($"No distance found for rotation {parentRotation} in object {currentTransform.name}");
                        }
                    }

                    totalDistance += partDistance;
                }
                else
                {
                    Debug.LogWarning($"Unable to parse distance from object name {currentTransform.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Unexpected object name format: {currentTransform.name}");
            }

            // If this part is a straight part, reset the parentRotation and lastCurvedRotation
            if (currentTransform.name.Contains("Straight"))
            {
                parentRotation = 0;
                lastCurvedRotation = 0;
            }
        }

        for (int i = 0; i < currentTransform.childCount; i++)
        {
            totalDistance += CalculateTotalDistance(currentTransform.GetChild(i), isStartPart, parentRotation, lastCurvedRotation);
        }

        return totalDistance;
    }

    float CalculateAverageSlope(Transform startTransform)
    {
        if (startTransform == null)
        {
            Debug.LogError("Start Transform is null"); // debug statement
            return 0f;
        }

        Transform endTransform = GetLastChild(startTransform);
        //Debug.Log("End Transform: " + endTransform.name); // debug statement

        float totalVerticalDrop = Mathf.Abs(startTransform.position.y - endTransform.position.y);
        //Debug.Log("Total Vertical Drop: " + totalVerticalDrop); // debug statement

        float totalDistance = CalculateTotalDistance(startTransform);
        //Debug.Log("Total Distance inside CalculateAverageSlope: " + totalDistance); // debug statement

        float averageSlopeRad = totalVerticalDrop / totalDistance;
        float averageSlopeDeg = Mathf.Rad2Deg * Mathf.Atan(averageSlopeRad);

        return totalDistance > 0 ? averageSlopeDeg : 0f;
    }

    private Transform GetLastChild(Transform parentTransform)
    {
        if (parentTransform.childCount == 0)
        {
            return parentTransform;
        }
        else
        {
            List<Transform> allChildren = new List<Transform>();
            foreach (Transform child in parentTransform)
            {
                allChildren.Add(GetLastChild(child));
            }
            return allChildren[allChildren.Count - 1];
        }
    }
    Vector3 CalculateBounds(Transform currentTransform)
    {
        // Initialize min and max coordinates
        Vector3 minCoordinates = currentTransform.position;
        Vector3 maxCoordinates = currentTransform.position;

        // Recurse into the children of the current object
        foreach (Transform child in currentTransform)
        {
            Vector3 childBounds = CalculateBounds(child);
            minCoordinates = Vector3.Min(minCoordinates, childBounds - childBounds);
            maxCoordinates = Vector3.Max(maxCoordinates, childBounds + childBounds);
        }

        // Width is in the X axis and length is in the Z axis
        return maxCoordinates - minCoordinates;
    }
    private int CountBeamObjects(Transform parentTransform)
    {
        int beamCount = 0;

        foreach (Transform child in parentTransform)
        {
            if (child.CompareTag("Beam"))
            {
                beamCount++;
            }

            beamCount += CountBeamObjects(child);
        }

        return beamCount;
    }
    public void CheckSlideLength()
    {
        int straightCount = 0;
        Transform slideStart = GameObject.Find("StartPart").transform;
        Transform current = slideStart;

        while (current != null)
        {
            if (current.tag == "Straight")
            {
                straightCount++;
                if (straightCount > 7)
                {
                    if (!annotationFlags.ContainsKey("Straight") || !annotationFlags["Straight"])
                    {
                        annotationScript.CreateAnnotation(AnnotationScript.AnnotationType.Error, "The maximum length of the first straight part of the slide area must not exceed 7,000 mm.", "Straight");
                        annotationFlags["Straight"] = true;
                    }
                    break;
                }
            }
            else
            {
                straightCount = 0;
                if (annotationFlags.ContainsKey("Straight") && annotationFlags["Straight"])
                {
                    annotationScript.HideAnnotation("Straight");
                    annotationFlags["Straight"] = false;
                }
            }
            if (current.childCount > 0)
            {
                current = current.GetChild(0);
            }
            else
            {
                current = null;
            }
        }
    }


    void CheckSlopeOfEachSection(Transform currentTransform)
    {
        if (currentTransform.CompareTag("Straight"))
        {
            // Find the SnapPoint of the current object
            Transform snapPoint = currentTransform.Find("SnapPoint");

            if (snapPoint != null)
            {
                float totalVerticalDrop = Mathf.Abs(currentTransform.position.y - snapPoint.position.y);
                Vector3 currentPos2D = new Vector3(currentTransform.position.x, 0, currentTransform.position.z);
                Vector3 snapPos2D = new Vector3(snapPoint.position.x, 0, snapPoint.position.z);
                float totalHorizontalDistance = Vector3.Distance(currentPos2D, snapPos2D);

                if (totalHorizontalDistance != 0) // To avoid division by zero.
                {
                    float slopeRad = totalVerticalDrop / totalHorizontalDistance;
                    float slopeDeg = Mathf.Rad2Deg * Mathf.Atan(slopeRad);

                    if (slopeDeg > 60)
                    {
                        string objectName = currentTransform.name;
                        bool isAnnotationCreated = annotationFlags.ContainsKey(objectName) && annotationFlags[objectName];

                        if (!isAnnotationCreated)
                        {
                            annotationScript.CreateAnnotation(AnnotationScript.AnnotationType.Error, "The slope angle of the slide area in relation to horizontal must not exceed 60°", objectName);
                            annotationFlags[objectName] = true;
                        }
                    }
                    else
                    {
                        string objectName = currentTransform.name;
                        if (annotationFlags.ContainsKey(objectName) && annotationFlags[objectName])
                        {
                            annotationScript.HideAnnotation(objectName);
                            annotationFlags[objectName] = false;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"No SnapPoint found for object {currentTransform.name}");
            }
        }

        // Recurse into the children of the current object
        for (int i = 0; i < currentTransform.childCount; i++)
        {
            CheckSlopeOfEachSection(currentTransform.GetChild(i));
        }
    }
    void Update()
    {
        if (startPart == null)
        {
            startPart = GameObject.Find("StartPart");
            
        }
        lastChild = GetLastChild(spawnStartScript.existingPrefab.transform);
        if (lastChild == null) return;

        string objectName = lastChild.name;  // Use this as the key for different types of annotations

        // Check if the annotation has already been created for the object
        bool isAnnotationCreated = annotationFlags.ContainsKey(objectName) && annotationFlags[objectName];

        if (averageSlope > 40)
        {
            if (!isAnnotationCreated)
            {
                annotationScript.CreateAnnotation(AnnotationScript.AnnotationType.Error, "The slope angle of the slide area in relation to horizontal must not exceed an average of 40°", objectName);
                annotationFlags[objectName] = true;
            }
        }
        else
        {
            if (isAnnotationCreated)
            {
                annotationScript.HideAnnotation(objectName);
                annotationFlags[objectName] = false;
            }
        }

        if (spawnStartScript.existingPrefab != null)
        {
            Transform snapPoint = spawnStartScript.existingPrefab.transform.Find("StartPart/SnapPoint");
            if (snapPoint != null)
            {
                bool straightExists = snapPoint.Cast<Transform>().Any(t => t.tag == "Straight");

                if (!straightExists)
                {
                    if (!annotationFlags.ContainsKey("SnapPoint") || !annotationFlags["SnapPoint"])
                    {
                        annotationScript.CreateAnnotation(AnnotationScript.AnnotationType.Error, "SnapPoint should have a child object with tag 'Straight'", "SnapPoint");
                        annotationFlags["SnapPoint"] = true;
                    }
                }
                else
                {
                    if (annotationFlags.ContainsKey("SnapPoint") && annotationFlags["SnapPoint"])
                    {
                        annotationScript.HideAnnotation("SnapPoint");
                        annotationFlags["SnapPoint"] = false;
                    }
                }
            }

            if (!infoannotation)
            {
                annotationScript.CreateAnnotation(AnnotationScript.AnnotationType.Information, "The support area around the slide's area should extend at least 1,000 mm.", "StartPart");
                annotationFlags["StartPart"] = true;
                infoannotation = true;
            }

            // Check the StartPart length
            if (startPart != null)
            {
                // Calculate length of StartPart in meters
                float rawlength = CalculateTotalDistance(startPart.transform);
                string length = (rawlength * 100).ToString("F0");

                // Calculate the required number of beams
                int requiredBeams = Mathf.FloorToInt(rawlength / 2.5f);

                // Count the number of beams in the StartPart
                int actualBeams = CountBeamObjects(startPart.transform);

                if (actualBeams < requiredBeams)
                {
                    if (!annotationFlags.ContainsKey("Beam") || !annotationFlags["Beam"])
                    {
                        annotationScript.CreateAnnotation(AnnotationScript.AnnotationType.Warning, $"Slide of length {length}cm should have at least {requiredBeams} beam(s). Currently has {actualBeams}.", "Beam");
                        annotationFlags["Beam"] = true;
                    }
                }
                else
                {
                    if (annotationFlags.ContainsKey("Beam") && annotationFlags["Beam"])
                    {
                        annotationScript.HideAnnotation("Beam");
                        annotationFlags["Beam"] = false;
                    }
                }
            }
        }
        else
        {
            if (infoannotation == true)
            {
                annotationScript.HideAnnotation("StartPart");
                annotationFlags["StartPart"] = false;
            }
        }
        GameObject endingObject = GameObject.Find("Ending");
        if (endingObject != null && endingObject.tag == "Straight")
        {
            float rotationTolerance = 5f; // Allowed rotation difference in degrees

            Vector3 worldUp = new Vector3(0, 1, 0);
            Vector3 worldDown = new Vector3(0, -1, 0);
            Vector3 objectUpInWorldSpace = endingObject.transform.TransformDirection(Vector3.up);

            float angleToUp = Vector3.Angle(objectUpInWorldSpace, worldUp);
            float angleToDown = Vector3.Angle(objectUpInWorldSpace, worldDown);

            if (angleToUp > rotationTolerance && angleToDown > rotationTolerance)
            {
                if (!annotationFlags.ContainsKey("Ending") || !annotationFlags["Ending"])
                {
                    annotationScript.CreateAnnotation(AnnotationScript.AnnotationType.Warning, "The Ending object should be parallel with the ground (rotation in relation to the world up vector or world down vector should be within ±5 degrees).", "Ending");
                    annotationFlags["Ending"] = true;
                }
            }
            else
            {
                if (annotationFlags.ContainsKey("Ending") && annotationFlags["Ending"])
                {
                    annotationScript.HideAnnotation("Ending");
                    annotationFlags["Ending"] = false;
                }
            }
        }
        if (startPart != null && lastChild != null)
        {
            string tunnelObjectName = "Tunnel";  // Use this as the key for different types of annotations

            // Check if the annotation has already been created for the object
            bool isTunnelAnnotationCreated = annotationFlags.ContainsKey(tunnelObjectName) && annotationFlags[tunnelObjectName];

            if (lastChild.position.y >= startPart.transform.position.y)
            {
                if (!isTunnelAnnotationCreated)
                {
                    annotationScript.CreateAnnotation(AnnotationScript.AnnotationType.Error, "The tunnel must slope downwards", tunnelObjectName);
                    annotationFlags[tunnelObjectName] = true;
                }
            }
            else
            {
                if (isTunnelAnnotationCreated)
                {
                    annotationScript.HideAnnotation(tunnelObjectName);
                    annotationFlags[tunnelObjectName] = false;
                }
            }
        }
        
    }
    IEnumerator CheckSlideLengthAndSlopeCoroutine()
    {
        while (true)
        {
            CheckSlideLength();
            CheckSlopeOfEachSection(startPart.transform);

            // wait for 2 seconds before the next check
            yield return new WaitForSeconds(2f);
        }
    }
}
