using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DistanceMeasurement : MonoBehaviour
{
    public enum MeasurementMode { PointToPoint, ObjectToObject }

    private MeasurementMode currentMode;
    private bool measurementEnabled = false;
    private bool measurementInProgress = false;
    private Vector3 startPoint;
    private Vector3 endPoint;
    public OrbitControls orbitControlsScript;
    public Material lineMaterial;
    private LineRenderer lineRenderer;
    public TextMeshProUGUI distanceText;
    public RawImage pointToPointButton;
    public RawImage objectToObjectButton;
    private Camera mainCamera;
    public RawImage pointToPointTexture;
    public Texture objectToObjectTexture;
    private GameObject startMarker;
    private GameObject endMarker;

    private bool isPointToPointActive = false;
    private bool isObjectToObjectActive = false;

    void Start()
    {
        mainCamera = Camera.main;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = lineMaterial;
        lineRenderer.enabled = false;

        gameObject.layer = LayerMask.NameToLayer("LineLayer");

        startMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        startMarker.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        startMarker.SetActive(false);

        // Destroy the collider of the start marker
        Destroy(startMarker.GetComponent<Collider>());

        endMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        endMarker.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        endMarker.SetActive(false);

        // Destroy the collider of the end marker
        Destroy(endMarker.GetComponent<Collider>());
    }

    void Update()
    {
        if (!measurementEnabled)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (measurementInProgress)
            {
                ResetMeasurement();
            }

            RaycastHit hitStart;
            if (currentMode == MeasurementMode.ObjectToObject && ObjectHit(out hitStart))
            {
                startPoint = hitStart.transform.position;
                startMarker.transform.position = startPoint;
                startMarker.SetActive(true);
            }
            else
            {
                startPoint = MouseToWorldPoint();
                startMarker.transform.position = startPoint;
                startMarker.SetActive(true);
            }

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.enabled = true;
            distanceText.enabled = true;

            // Disable camera movement
            orbitControlsScript.enabled = false;

            measurementInProgress = true;
        }

        if (Input.GetMouseButton(0) && measurementInProgress && currentMode == MeasurementMode.PointToPoint)
        {
            Vector3 tempEndPoint = MouseToWorldPoint();
            lineRenderer.SetPosition(1, tempEndPoint);
            endMarker.transform.position = tempEndPoint;
            endMarker.SetActive(true);

            float distance = Vector3.Distance(startPoint, tempEndPoint);
            distanceText.text = "Distance: " + (distance*100).ToString("F0") + "cm";

            Vector3 midPoint = (startPoint + tempEndPoint) / 2;
            midPoint.y += 0.1f;
            distanceText.transform.position = mainCamera.WorldToScreenPoint(midPoint);
        }

        if (Input.GetMouseButtonUp(0) && measurementInProgress)
        {
            RaycastHit hitEnd;
            if (currentMode == MeasurementMode.ObjectToObject && ObjectHit(out hitEnd))
            {
                endPoint = hitEnd.transform.position;
                endMarker.transform.position = endPoint;
                endMarker.SetActive(true);
            }
            else
            {
                endPoint = MouseToWorldPoint();
                endMarker.transform.position = endPoint;
                endMarker.SetActive(true);
            }

            lineRenderer.SetPosition(1, endPoint);

            float distance = Vector3.Distance(startPoint, endPoint);
            distanceText.text = "Distance: " + (distance * 100).ToString("F0") + "cm";

            Vector3 midPoint = (startPoint + endPoint) / 2;
            midPoint.y += 0.1f;
            distanceText.transform.position = mainCamera.WorldToScreenPoint(midPoint);

            measurementInProgress = false;
            orbitControlsScript.enabled = true; // Enable camera movement after measurement
        }
    }



    bool ObjectHit(out RaycastHit hitResult)
    {
        int layerMask = 1 << LayerMask.NameToLayer("SnappedObjects");
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitResult, Mathf.Infinity, layerMask))
        {
            return true;
        }
        return false;
    }

    Vector3 MouseToWorldPoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(-mainCamera.transform.forward, startPoint);

        float enter;
        if (plane.Raycast(ray, out enter))
        {
            return ray.GetPoint(enter);
        }

        return Vector3.zero;
    }


    public void ToggleMeasurement()
    {
        // If measurement tool is turned off, ensure camera movement is enabled
        if (measurementEnabled)
        {
            orbitControlsScript.enabled = true;
            measurementEnabled = false;
            ResetMeasurement();
        }
        else
        {
            orbitControlsScript.enabled = false;
            measurementEnabled = true;
            ResetMeasurement();
        }
    }

    public void StartPointToPointMeasurement()
    {
        if (isObjectToObjectActive)
        {
            isObjectToObjectActive = false;
            ResetMeasurement();
        }
        isPointToPointActive = !isPointToPointActive; // toggle state
        measurementEnabled = isPointToPointActive;  // set measurementEnabled flag based on isPointToPointActive
        if (isPointToPointActive)
        {
            currentMode = MeasurementMode.PointToPoint;
            pointToPointTexture.color = Color.green;
            StartMeasurement();
        }
        else
        {
            ResetMeasurement();
            orbitControlsScript.enabled = true; // Enable camera movement when measurement is off
            pointToPointTexture.color = Color.white;
        }
    }

    public void StartObjectToObjectMeasurement()
    {
        if (isPointToPointActive)
        {
            isPointToPointActive = false;
            ResetMeasurement();
        }
        isObjectToObjectActive = !isObjectToObjectActive; // toggle state
        measurementEnabled = isObjectToObjectActive;  // set measurementEnabled flag based on isObjectToObjectActive
        if (isObjectToObjectActive)
        {
            currentMode = MeasurementMode.ObjectToObject;
            StartMeasurement();
        }
        else
        {
            ResetMeasurement();
            orbitControlsScript.enabled = true; // Enable camera movement when measurement is off
        }
    }


    private void ResetMeasurement()
    {
        lineRenderer.enabled = false;
        distanceText.enabled = false;
        startMarker.SetActive(false);
        endMarker.SetActive(false);
    }

    private void StartMeasurement()
    {
        if (!measurementEnabled && (isPointToPointActive || isObjectToObjectActive))
        {
            orbitControlsScript.enabled = false;
            measurementEnabled = true;
            ResetMeasurement();
        }
    }
}
