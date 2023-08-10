using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using System.Text;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class CaptureScreenshot : MonoBehaviour
{
    public Camera captureCamera;
    public Camera panelCamera1;
    public Camera panelCamera2;
    public Camera panelCamera3;
    public Camera panelCamera4;
    public GameObject a4Template;
    public GameObject mainCamera;
    public GameObject uiCanvas;
    public GameObject annotationsCanvas;
    public int textureWidth = 2100;
    public RawImage panelImage1;
    public RawImage panelImage2;
    public RawImage panelImage3;
    public RawImage panelImage4;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI createdByText;
    private GameObject emptyObject;
    public List<GameObject> modelObjects = new List<GameObject>();
    public GameObject startPart;
    Dictionary<GameObject, float> modelObjectsWithParentRotationY = new Dictionary<GameObject, float>();
    Dictionary<GameObject, Color> modelObjectsMaterialColor = new Dictionary<GameObject, Color>();


    public Texture straightIcon;
    public Texture windowIcon;
    public Texture endPipeIcon;
    public Texture endMiddleIcon;
    public Texture endendIcon;
    public Texture doubleSupport;
    public Texture singleSupport;
    public Texture middleSupport;

    public GameObject objectsParent;
    public UIManager uiManager;
    private LoadGameObjectController loadController;
    private ObjectRotator rotator;


    private void Start()
    {
        rotator = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
        loadController = gameObject.transform.GetComponent<LoadGameObjectController>();
    }
    public void Capture()
    {
        Debug.Log("Capture method called");
        if (rotator.selectedObject != null || rotator.selectedObject != null)
        {
            rotator.selectedObject.GetComponent<Outline>().enabled = false;
            rotator.selectedObject = GameObject.Find("GUISave");
        }

        StartCoroutine(CaptureAndSaveCoroutine());
    }

    private IEnumerator CaptureAndSaveCoroutine()
    {

        //yield return StartCoroutine(CaptureAndSetTexture(panelCamera1, panelImage1));
        //yield return StartCoroutine(CaptureAndSetTexture(panelCamera2, panelImage2));
        // yield return StartCoroutine(CaptureAndSetTexture(panelCamera3, panelImage3));
        //yield return StartCoroutine(CaptureAndSetTexture(panelCamera4, panelImage4));

        yield return StartCoroutine(CaptureAndSaveImage(captureCamera));
    }

    private IEnumerator CaptureAndSetTexture(Camera camera, RawImage panel)
    {
        yield return CaptureCoroutine(camera, (result) =>
        {
            panel.texture = result;
        }, isPanelCamera: true);
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator CaptureAndSaveImage(Camera camera)
    {
        string slideId = loadController.loadedSlideId.ToString();
        if (string.IsNullOrEmpty(slideId))
        {
            slideId = System.DateTime.Now.ToString("yyyy-MM-dd");
        }
        // Save the original position of the camera
        Vector3 originalPosition = camera.transform.localPosition;

        // Move the camera to the first position
        camera.transform.localPosition = new Vector3(-6598f, camera.transform.localPosition.y, camera.transform.localPosition.z);

        string firstScreenshot = null;
        yield return SaveScreenshot(camera, "screenshot.png", result => { firstScreenshot = System.Convert.ToBase64String(result.EncodeToPNG()); Destroy(result); });

        // Restore the original position of the camera
        camera.transform.localPosition = originalPosition;

        // Move the camera to the second position
        camera.transform.localPosition = new Vector3(-4305f, camera.transform.localPosition.y, camera.transform.localPosition.z);

        string secondScreenshot = null;
        yield return SaveScreenshot(camera, "screenshot2.png", result => { secondScreenshot = System.Convert.ToBase64String(result.EncodeToPNG()); Destroy(result); });

        // Restore the original position of the camera (optional)
        camera.transform.localPosition = originalPosition;

        // Call SaveImagesAsPDF() JavaScript function
        Application.ExternalEval($"SaveImagesAsPDF('screenshot.png', '{firstScreenshot}', 'screenshot2.png', '{secondScreenshot}', '{slideId}')");
    }
    private IEnumerator SaveScreenshot(Camera camera, string filename, System.Action<Texture2D> callback)
    {
        string date = System.DateTime.Now.ToString("dd-MM-yyyy");
        dateText.text = "Printed: " + date;
        GameObject startPart = GameObject.Find("StartPart");
        string height = (((startPart.transform.position.y) - 0.3805935f) * 100).ToString("F0");
        heightText.text = "Start Height: " + height + " cm";
        createdByText.text = "Created by: " + uiManager.accountUsername;

        yield return CaptureCoroutine(camera, (result) =>
        {
            if (Application.isEditor)
            {
                byte[] pngData = result.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + "/" + filename, pngData);
            }
            else
            {
                callback(result);
            }
        });
    }

    private IEnumerator CaptureCoroutine(Camera camera, System.Action<Texture2D> onFinished, bool isPanelCamera = false)
    {
        a4Template.SetActive(true);
        mainCamera.SetActive(false);
        annotationsCanvas.SetActive(false);

        camera.enabled = true;

        int textureHeight = Mathf.RoundToInt(textureWidth * 1.4142f);

        RenderTexture rt = new RenderTexture(textureWidth, textureHeight, 24);
        camera.targetTexture = rt;

        yield return new WaitForEndOfFrame();

        camera.Render();

        RenderTexture.active = rt;

        int cropSize = Mathf.Min(textureWidth, textureHeight);
        float panelAspect = 1.21495172f;
        if (isPanelCamera)
        {
            if (textureHeight < textureWidth)
            {
                cropSize = Mathf.RoundToInt(textureHeight * panelAspect);
            }
            else
            {
                cropSize = Mathf.RoundToInt(textureWidth / panelAspect);
            }
        }

        Rect cropRect = new Rect((textureWidth - cropSize) / 2, (textureHeight - cropSize) / 2, cropSize, cropSize);
        Rect readRect = isPanelCamera ? cropRect : new Rect(0, 0, textureWidth, textureHeight);
        int readWidth = Mathf.RoundToInt(readRect.width);
        int readHeight = Mathf.RoundToInt(readRect.height);

        Texture2D tex = new Texture2D(readWidth, readHeight, TextureFormat.RGB24, false);
        tex.ReadPixels(readRect, 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        camera.targetTexture = null;
        Destroy(rt);

        camera.enabled = false;

        mainCamera.SetActive(true);
        annotationsCanvas.SetActive(true);

        onFinished?.Invoke(tex);
        a4Template.SetActive(false);
    }

    void FindModels(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                // Handle model objects
                if (child.gameObject.name.StartsWith("model") && !child.gameObject.name.StartsWith("model-startPart"))
                {
                    // Convert and store the GameObject and its parent's local y rotation
                    float rotation = ConvertRotation(child.parent.localRotation.eulerAngles.y);
                    modelObjectsWithParentRotationY[child.gameObject] = rotation;

                    // Store the GameObject and its material color
                    Renderer renderer = child.gameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        modelObjectsMaterialColor[child.gameObject] = renderer.material.color;
                    }
                }
                // Handle SupportBeam objects
                else if (child.gameObject.name.StartsWith("SupportBeam"))
                {
                    // Store the GameObject reference with special rotation value
                    modelObjectsWithParentRotationY[child.gameObject] = float.NaN;
                }

                // Recursively call for each child regardless of their name or activity status
                FindModels(child);
            }
        }
    }


    public void LookForModel()
    {
        modelObjectsWithParentRotationY.Clear();
        modelObjectsMaterialColor.Clear();

        if (startPart == null)
        {

            startPart = GameObject.Find("StartPart");
            FindModels(startPart.transform);
            DisplayModelObjects();
            ModifyObjects();
        }
        else
        {
            FindModels(startPart.transform);
            DisplayModelObjects();
            ModifyObjects();
        }
    }

    public void DisplayModelObjects()
    {
        string logMessage = "Model Objects and Their Parent's Local Y Rotation: ";

        foreach (KeyValuePair<GameObject, float> entry in modelObjectsWithParentRotationY)
        {
            logMessage += "\n" + entry.Key.name + " - " + entry.Value;
        }

        Debug.Log(logMessage);
    }

    void ModifyObjects()
    {
        int i = 1;
        foreach (KeyValuePair<GameObject, float> entry in modelObjectsWithParentRotationY)
        {
            // Find the object (Object1, Object2,...)
            GameObject obj = objectsParent.transform.Find("Object" + i).gameObject;
            if (obj == null)
            {
                Debug.LogError("Object" + i + " does not exist under objectsParent.");
                continue; // Skip this iteration of the loop
            }


            // Change the texture of its child "Standard"
            RawImage rawImage = obj.transform.Find("Standard").GetComponent<RawImage>();
            if (entry.Key.name.StartsWith("model-straight"))
            {
                // Check if the object name contains "window" and use the windowIcon
                if (entry.Key.name.Contains("window"))
                {
                    rawImage.texture = windowIcon;
                }
                else
                {
                    rawImage.texture = straightIcon;
                }
            }
            else if (entry.Key.name.StartsWith("model-curved"))
            {
                rawImage.texture = straightIcon;
            }
            else if (entry.Key.name.StartsWith("model-ending"))
            {
                if (entry.Key.name.Contains("middle"))
                {
                    rawImage.texture = endMiddleIcon;
                }
                else if (entry.Key.name.Contains("model-ending-end"))
                {
                    rawImage.texture = endendIcon;
                }
                else
                {

                    rawImage.texture = endPipeIcon;
                }

            }

            else if (entry.Key.name.StartsWith("SupportBeam"))
            {
                if (entry.Key.name.Contains("single"))
                {
                    rawImage.texture = singleSupport;
                }
                else if (entry.Key.name.Contains("middle"))
                {
                    rawImage.texture = middleSupport;
                }
                else
                {
                    rawImage.texture = doubleSupport;
                }
            }
            if (!entry.Key.name.StartsWith("SupportBeam"))
            {
                rawImage.color = modelObjectsMaterialColor[entry.Key];  // Apply material color

            }
            int snapPointYRotation = 0;
            // Modify the text of the grandchildren "Size" and "SnapPointNut"
            TextMeshProUGUI sizeText = obj.transform.Find("Standard/Size").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI SnapPointNutText = obj.transform.Find("Standard/SnapPointNut").GetComponent<TextMeshProUGUI>();
            Transform snapPointTransform = entry.Key.transform.parent.Find("SnapPoint");
            if (snapPointTransform != null && snapPointTransform.childCount > 0)
            {
                Transform snapPointChild = snapPointTransform.GetChild(0);
                float snapPointChildYRotation = snapPointChild.transform.localRotation.eulerAngles.y;
                int YRotation = Mathf.RoundToInt((snapPointChildYRotation + 180f) / 15f) % 24;
                if (YRotation == 0)
                {
                    YRotation = 24;
                }
                if (snapPointChild.CompareTag("Curved"))
                {
                    YRotation = 12;
                }
                Debug.Log(snapPointChild.name + "Rotation: " + YRotation);
                snapPointYRotation = YRotation;


            }
            if (entry.Key.name.StartsWith("model-ending"))
            {
                sizeText.text = null;
                SnapPointNutText.text = null;

            }
            if (entry.Key.name.StartsWith("SupportBeam"))
            {
                if (entry.Key.name.Contains("single"))
                {
                    Transform SupportBeamEndPoint = entry.Key.transform.GetChild(0).transform.GetChild(0).transform;
                    GameObject StartPart = GameObject.Find("StartPart");

                    // Get the position of the endpoint relative to StartPart
                    Vector3 relativePos = StartPart.transform.InverseTransformPoint(SupportBeamEndPoint.position);

                    // Convert from meters to centimeters
                    float diggingPosX = relativePos.x * 100; // left/right distance in cm
                    float diggingPosZ = relativePos.z * 100; // forward/backward distance in cm

                    // Calculate and format the display text
                    string sideDirection = diggingPosX >= 0 ? "R" : "L";
                    sizeText.text = $"F{Mathf.Abs(diggingPosZ):0},{sideDirection}{Mathf.Abs(diggingPosX):0}";
                    sizeText.fontSize = 9;
                    SnapPointNutText.text = null;
                }
                else if (entry.Key.name.Contains("double"))
                {
                    Transform SupportBeamEndPoint1 = entry.Key.transform.GetChild(0).transform.GetChild(0).transform;
                    Transform SupportBeamEndPoint2 = entry.Key.transform.GetChild(1).transform.GetChild(0).transform;
                    GameObject StartPart = GameObject.Find("StartPart");
                    Vector3 relativePos1 = StartPart.transform.InverseTransformPoint(SupportBeamEndPoint1.position);
                    Vector3 relativePos2 = StartPart.transform.InverseTransformPoint(SupportBeamEndPoint2.position);

                    float diggingPosX1 = relativePos1.x * 100; // left/right distance in cm
                    float diggingPosZ1 = relativePos1.z * 100;

                    float diggingPosX2 = relativePos2.x * 100; // left/right distance in cm
                    float diggingPosZ2 = relativePos2.z * 100;

                    string sideDirection1 = diggingPosX1 <= 0 ? "R" : "L";
                    string sideDirection2 = diggingPosX2 <= 0 ? "R" : "L";
                    sizeText.fontSize = 9;
                    sizeText.text = $"F{Mathf.Abs(diggingPosZ1):0},{sideDirection1}{Mathf.Abs(diggingPosX1):0}|F{Mathf.Abs(diggingPosZ2):0},{sideDirection2}{Mathf.Abs(diggingPosX2):0}";
                }
                
            }


            else if(!entry.Key.name.StartsWith("model-ending") && !entry.Key.name.StartsWith("SupportBeam"))
            {
                string unit = entry.Key.name.StartsWith("model-curved") ? "°" : "cm";
                sizeText.text = entry.Key.name.Split('-')[2] + unit;
                
            }

            TextMeshProUGUI originNutText = obj.transform.Find("Standard/OriginNut").GetComponent<TextMeshProUGUI>();

            // Check if it's a "model-straight" object and set the text as "A"
            if (entry.Key.name.StartsWith("model-straight"))
            {
                originNutText.text = "A";
                SnapPointNutText.text = "A";
            }
            else if (entry.Key.name.StartsWith("SupportBeam"))
            {
                originNutText.text = "";
                SnapPointNutText.text = "";
                if (entry.Key.name.Contains("middle"))
                {
                    sizeText.text = "";
                }

            }
            else if (entry.Key.name.Contains("model-ending-middle"))
            {
                originNutText.text = "";
            }
            else if (entry.Key.name.Contains("model-ending-end"))
            {
                originNutText.text = "";
            }
            else if (entry.Key.name.Contains("model-ending"))
            {
                sizeText.text = "";
                SnapPointNutText.text = "";
                originNutText.text = "A";
            }
            else
            {

                int originNutYRotation = Mathf.RoundToInt((entry.Value + 180f) / 15f) % 24;
                if (originNutYRotation == 0)
                {
                    originNutYRotation = 24;
                }
                originNutText.text = originNutYRotation.ToString();
                SnapPointNutText.text = snapPointYRotation.ToString();
            }

            obj.SetActive(true);
            i++;
        }

        // Disable the remaining unused objects
        for (; i <= 40; i++)
        {
            GameObject obj = objectsParent.transform.Find("Object" + i).gameObject;
            obj.SetActive(false);
        }
    }

    string RotationToClockFormat(float rotation)
    {
        // Map -180 to 180 degrees to 1 to 24
        int newRotation = (int)Mathf.Round(((rotation + 180) / 15) + 1);
        return newRotation.ToString();
    }
    public void ResetAndEnableObjects()
    {
        for (int i = 0; i < 40; i++)
        {
            Transform childTransform = objectsParent.transform.GetChild(i);

            // Enable the object
            childTransform.gameObject.SetActive(true);

            // Reset the image and text properties if they exist
            Transform standardTransform = childTransform.Find("Standard");
            if (standardTransform != null)
            {
                RawImage rawImage = standardTransform.GetComponent<RawImage>();
                if (rawImage != null)
                {
                    rawImage.texture = straightIcon;
                    rawImage.color = Color.white;
                }

                // Reset text properties
                TextMeshProUGUI sizeText = standardTransform.Find("Size")?.GetComponent<TextMeshProUGUI>();
                if (sizeText != null)
                {
                    sizeText.text = "";
                    sizeText.fontSize = 18;
                }
                TextMeshProUGUI SnapPointNutText = standardTransform.Find("SnapPointNut")?.GetComponent<TextMeshProUGUI>();
                if (SnapPointNutText != null)
                    SnapPointNutText.text = "";

                TextMeshProUGUI originNutText = standardTransform.Find("OriginNut")?.GetComponent<TextMeshProUGUI>();
                if (originNutText != null)
                    originNutText.text = "";
            }
        }
    }

    float ConvertRotation(float rotation)
    {
        if (rotation > 180)
        {
            rotation -= 360;
        }

        return rotation;
    }
}
