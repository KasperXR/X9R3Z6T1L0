using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using Dummiesman.Extensions;

public class ExportCSV : MonoBehaviour
{
    public string startPartName = "StartPart";
    public string filePath = "Assets/ExportedData.csv";

    private Transform startPart;
    private GameObject emptyObject;
    private Vector3 originalScale;
    private StringBuilder csvBuilder;
    private StraightPieceAdjust[] straightPieceAdjusts;
    private LoadGameObjectController loadGameObjectController;
    // Create a dictionary for specific name transformations at class level
    private Dictionary<string, string> nameTransformations = new Dictionary<string, string>
    {
        { "model-straight-35", "6602" },
        { "model-straight-59", "6604" },
        { "model-straight-59-window", "6605" },
        { "model-straight-106", "6606" },
        { "model-straight-106-window", "6607" },
        { "model-curved-32", "6610" },
        { "model-curved-38", "6612" },
        { "model-curved-45", "6614" },
        { "model-startPart-standard", "2000" },
        { "model-startPart-curved", "3000" },
        { "model-ending", "6616" },
        { "model-ending-middle", "6636" },
        { "model-ending-end", "6637" },
        { "Pole(Clone)", "6600" },
    };
    private Dictionary<Color, int> colorIndexMapping = new Dictionary<Color, int>
{
    { new Color(253f/255f, 182f/255f, 2f/255f), 1 },
    { new Color(204f/255f, 31f/255f, 31f/255f), 2 },
    { new Color(59f/255f, 113f/255f, 55f/255f), 3 },
    { new Color(52f/255f, 77f/255f, 200f/255f), 4 },
    { new Color(153f/255f, 153f/255f, 153f/255f), 5 }
};
    private void Start()
    {
        loadGameObjectController = gameObject.transform.GetComponent<LoadGameObjectController>();
    }
    public void Export()
    {
        straightPieceAdjusts = FindObjectsOfType<StraightPieceAdjust>();

        foreach (StraightPieceAdjust script in straightPieceAdjusts)
        {
            script.enabled = false;
        }

        GameObject startPartObject = GameObject.Find(startPartName);
        if (startPartObject != null)
        {
            csvBuilder = new StringBuilder();
            startPart = startPartObject.transform;
            originalScale = startPart.localScale;
            startPart.localScale = new Vector3(-1, startPart.localScale.y, startPart.localScale.z);

            StartCoroutine(ExportAndResetScale(startPart));
        }
        else
        {
            Debug.LogWarning("StartPart not found. CSV export failed.");
        }
    }

    private IEnumerator ExportAndResetScale(Transform startPart)
    {
        yield return StartCoroutine(GenerateAndSaveCSV(startPart));
        string csvData = csvBuilder.ToString();
        string slideID = loadGameObjectController.loadedSlideId.ToString();
        DownloadCSV(csvData, slideID);
        startPart.localScale = originalScale;

        foreach (StraightPieceAdjust script in straightPieceAdjusts)
        {
            script.enabled = true;
        }
    }

    private IEnumerator GenerateAndSaveCSV(Transform startPart)
    {
        csvBuilder.AppendLine("\"Name\";\"Matrix1X\";\"Matrix1Y\";\"Matrix1Z\";\"Matrix1W\";\"Matrix2X\";\"Matrix2Y\";\"Matrix2Z\";\"Matrix2W\";\"Matrix3X\";\"Matrix3Y\";\"Matrix3Z\";\"Matrix3W\";\"Matrix4X\";\"Matrix4Y\";\"Matrix4Z\";\"Matrix4W\"");


        yield return StartCoroutine(ProcessTransform(startPart, csvBuilder));
    }

    public void DownloadCSV(string csvData, string slideId)
    {
        string filename = "Ledon";
        if (!string.IsNullOrEmpty(slideId))
        {
            filename += "_" + slideId;
        }
        else
        {
            filename += "_" + DateTime.Now.ToString("yyyy_MM_dd");
        }
        filename += ".csv";

        if (Application.isEditor)
        {
            SaveCSVToFile(csvData, filename);
        }
        else
        {
            string formattedCsvData = csvData.Replace("\n", "\\n").Replace("\"", "\\\"");

            string jsCode = $@"
        var csvData = new Blob(['{formattedCsvData}'], {{type: 'text/csv'}});
        var url = URL.createObjectURL(csvData);
        var link = document.createElement('a');
        link.href = url;
        link.download = '{filename}';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        ";

            Application.ExternalEval(jsCode);
        }
    }

    private IEnumerator ProcessTransform(Transform t, StringBuilder csvBuilder)
    {

        if (t.gameObject.activeInHierarchy && t.name != "Origin_nuts" && t.name != "SnapPoint_nuts")
        {

            if (t.name.StartsWith("model", StringComparison.OrdinalIgnoreCase) || t.tag == "Pole")
            {

                string newName = t.name;

                if (nameTransformations.TryGetValue(t.name, out string transformedName))
                {
                    newName = transformedName;
                }

                if (emptyObject == null)
                {
                    emptyObject = new GameObject("EmptyObject");
                }
                if (t.tag == "Pole")
                {
                    emptyObject.transform.SetParent(t.Find("StartPoint"));
                    emptyObject.transform.localPosition = Vector3.zero;
                    emptyObject.transform.localRotation = Quaternion.identity;
                    Debug.Log("StartPoint has postion " + emptyObject.transform.position);

                }
                else
                {
                    emptyObject.transform.SetParent(t.parent);
                    emptyObject.transform.localPosition = Vector3.zero;
                    emptyObject.transform.localRotation = Quaternion.identity;
                    emptyObject.transform.Rotate(0f, 180f, 0f, Space.Self);

                }
                Matrix4x4 matrix = emptyObject.transform.localToWorldMatrix;
                MeshRenderer meshRenderer = t.GetComponent<MeshRenderer>();
                Color color = Color.white;

                // Checking if "Origin_nuts" GameObject exists
                Transform originNuts = t.parent.Find("Origin_nuts");
                MeshRenderer fibreRenderer = null;
                Color fibreColor = Color.white;
                if (originNuts != null)
                {
                    fibreRenderer = originNuts.GetComponent<MeshRenderer>();
                    fibreColor = fibreRenderer.material.color;
                }
                else
                {
                    Debug.Log("Origin_nuts not found for " + t.name);
                }

                Color windowColor = Color.white;

                if (meshRenderer != null)
                {

                    color = meshRenderer.sharedMaterial.color;


                    foreach (var material in meshRenderer.materials)
                    {


                        if (material.name.StartsWith("Window"))
                        {
                            windowColor = material.color;

                        }
                    }

                }
                else
                {
                    Debug.Log("No MeshRenderer found for model: " + t.gameObject.name);
                }

                string colorIndex = "";
                if (color == new Color(249 / 255f, 168 / 255f, 0)) // RGB: 249, 168, 0
                {
                    if (t.name != "model-startPart-standard" && t.name != "model-startPart-curved")
                    {
                        colorIndex = "1";
                    }
                    else
                    {
                        colorIndex = "0772";
                    }
                }
                else if (color == new Color(187 / 255f, 30 / 255f, 16 / 255f)) // RGB: 187, 30, 16
                {
                    if (t.name != "model-startPart-standard" && t.name != "model-startPart-curved")
                    {
                        colorIndex = "2";
                    }
                    else
                    {
                        colorIndex = "0771";
                    }
                }
                else if (color == new Color(0, 111 / 255f, 61 / 255f)) // RGB: 0, 111, 61
                {
                    if (t.name != "model-startPart-standard" && t.name != "model-startPart-curved")
                    {
                        colorIndex = "3";
                    }
                    else
                    {
                        colorIndex = "0773";
                    }
                }
                else if (color == new Color(31 / 255f, 56 / 255f, 85 / 255f)) // RGB: 31, 56, 85
                {
                    if (t.name != "model-startPart-standard" && t.name != "model-startPart-curved")
                    {
                        colorIndex = "4";
                    }
                    else
                    {
                        colorIndex = "0775";
                    }
                }
                else if (color == new Color(87 / 255f, 93 / 255f, 94 / 255f)) // RGB: 87, 93, 94
                {
                    if (t.name != "model-startPart-standard" && t.name != "model-startPart-curved")
                    {
                        colorIndex = "5";
                    }
                    else
                    {
                        colorIndex = "0774";
                    }
                }
                string fibreIndex = "";
                if (fibreColor == new Color(253f / 255f, 182f / 255f, 2f / 255f))
                {
                    fibreIndex = "1";
                }
                else if (fibreColor == new Color(204f / 255f, 31f / 255f, 31f / 255f))
                {
                    fibreIndex = "2";
                }
                else if (fibreColor == new Color(59f / 255f, 113f / 255f, 55f / 255f))
                {
                    fibreIndex = "3";
                }
                else if (fibreColor == new Color(52f / 255f, 77f / 255f, 200f / 255f))
                {
                    fibreIndex = "4";
                }
                else if (fibreColor == new Color(153f / 255f, 153f / 255f, 153f / 255f))
                {
                    fibreIndex = "5";
                }
                string windowIndex = "";
                if (windowColor == new Color(226f / 255f, 163f / 255f, 0f / 255f))
                {
                    windowIndex = "1";
                }
                else if (windowColor == new Color(187f / 255f, 30f / 255f, 16f / 255f))
                {
                    windowIndex = "2";
                }
                else if (windowColor == new Color(77f / 255f, 111f / 255f, 57f / 255f))
                {
                    windowIndex = "3";
                }
                else if (windowColor == new Color(0f / 255f, 79f / 255f, 124f / 255f))
                {
                    windowIndex = "4";
                }
                else if (windowColor == new Color(152f / 255f, 158f / 255f, 161f / 255f))
                {
                    windowIndex = "5";
                }

                if (t.tag == "Pole")
                {

                }
                else
                {
                    newName += "/" + colorIndex;
                }

                var customCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ",";
                string csvLine = string.Format(customCulture,
                 "\"{0}\";\"{1:F6}\";\"{2:F6}\";\"{3:F6}\";\"{4:F6}\";\"{5:F6}\";\"{6:F6}\";\"{7:F6}\";\"{8:F6}\";\"{9:F6}\";\"{10:F6}\";\"{11:F6}\";\"{12:F6}\";\"{13:F6}\";\"{14:F6}\";\"{15:F6}\";\"{16:F6}\"",
                 newName,
                 matrix.m00, matrix.m10, matrix.m20, matrix.m30,
                 matrix.m01, matrix.m11, matrix.m21, matrix.m31,
                 matrix.m02, matrix.m12, matrix.m22, matrix.m32,
                 matrix.m03, matrix.m13, matrix.m23, matrix.m33);

                csvBuilder.AppendLine(csvLine);

                string[] excludedObjects = new[] { "model-ending", "6616", "model-ending-middle", "6636", "model-ending-end", "6637", "Pole(Clone)", "6600" };

                // Append the same line but with the "+fibre" suffix
                if (!Array.Exists(excludedObjects, element => element == t.name))
                {
                    string fibreName = newName.Substring(0, newName.LastIndexOf('/')) + "-fibre/" + fibreIndex; // trim off the ",1" and append "-fibre"
                    string fibreLine = csvLine.Replace(newName, fibreName);
                    csvBuilder.AppendLine(fibreLine);
                }

                // Add more rows if the object is specific
                string[] specificObjects = new[] { "6605", "6607" }; // List your specific objects here
                if (Array.Exists(specificObjects, element => element == newName.Substring(0, newName.LastIndexOf('/'))))
                {
                    string[] extraRows = new[] { "-frame/" + windowIndex, "-glass", "-bolts" };
                    foreach (var extraRow in extraRows)
                    {
                        string extraName = newName.Substring(0, newName.LastIndexOf('/')) + extraRow;
                        string extraLine = csvLine.Replace(newName, extraName);
                        csvBuilder.AppendLine(extraLine);
                    }
                }
            }
        }
        for (int i = 0; i < t.childCount; i++)
        {
            yield return StartCoroutine(ProcessTransform(t.GetChild(i), csvBuilder));
        }
    }

    public void SaveCSVToFile(string csvData, string filename)
    {
        {
            try
            {
                File.WriteAllText(filePath, csvData);
                Debug.Log("CSV data saved to " + filePath);
            }
            catch (Exception e)

            {
                Debug.LogError("Failed to save CSV data to file. Error: " + e.Message);
            }
        }
    }
}

