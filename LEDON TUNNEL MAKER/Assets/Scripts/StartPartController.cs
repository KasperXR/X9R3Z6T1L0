using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using ES3Types;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Unity.VisualScripting;
using TMPro;

public class StartPartController : MonoBehaviour
{
    public GameObject startPart;
    private int currentDataIndex = 0;
    private int maxDataLimit = 10;
    public string filename;
    public OrbitControls orbitControls;
    public UIManager uiManager;
    public GeneratePricingList generatePricingList;
    public List<string> savedFiles = new List<string>();
    public TMP_Text warningTextSave;
    public Camera axoCamera;
    private bool isLoadingData = false;
    private Queue<Action> saveQueue = new Queue<Action>();
    private bool isSaving = false;
    public GameObject savePanel;
    private HierarchyBounds bounds;
    public GameObject notificationPanel;
    private LoadGameObjectController loadController;
    private Indicator indicator;
    public TMP_InputField filenameInput;
    private ObjectRotator rotator;


    private void Start()
    {
        bounds = GameObject.Find("CommandManager").GetComponent<HierarchyBounds>();
        loadController = gameObject.GetComponent<LoadGameObjectController>();
        indicator = GameObject.Find("Indicator").GetComponent<Indicator>();
        rotator = GameObject.Find("RotatorScript").GetComponent<ObjectRotator>();
    }
    public void SetStartPart(GameObject newStartPart)
    {
        startPart = newStartPart;
    }

    public static List<GameObject> GetAllChildren(GameObject gameObject)
    {
        List<GameObject> result = new List<GameObject>();

        result.Add(gameObject);

        foreach (Transform child in gameObject.transform)
        {
            result.AddRange(GetAllChildren(child.gameObject));
        }

        return result;
    }
    public void SaveData()
    {
        if (startPart == null)
        {
            Debug.LogError("StartPart not assigned.");
            startPart = GameObject.Find("StartPart");
        }

        // Enqueue a new save operation
        saveQueue.Enqueue(() => {
            // Delete all save states after the currentDataIndex
            for (int i = savedFiles.Count - 1; i > currentDataIndex; i--)
            {
                ES3.DeleteFile(savedFiles[i]);
                savedFiles.RemoveAt(i);
            }

            // Now currentDataIndex points to the last save state, so increment it for the new save state
            currentDataIndex++;
            List<GameObject> undoPrefabs = GatherTaggedPrefabs(startPart);
            string filename = "startPart" + currentDataIndex;
            ES3Settings settings = new ES3Settings { location = ES3.Location.Cache};
            ES3.Save<List<GameObject>>("startPart", undoPrefabs, filename, settings);

            // Add filename to savedFiles
            savedFiles.Add(filename);

            // If the total number of saved files exceeds the max limit
            if (savedFiles.Count > maxDataLimit)
            {
                // Delete the oldest file
                ES3.DeleteFile(savedFiles[0]);
                // Remove the oldest file name from savedFiles list
                savedFiles.RemoveAt(0);
                // Decrement currentDataIndex since we removed the first save state
                currentDataIndex--;
            }

            isSaving = false;
        });

        // If not currently saving, start the save processor coroutine
        if (!isSaving)
        {
            StartCoroutine(SaveProcessor());
        }
    }
    private IEnumerator SaveProcessor()
    {
        while (saveQueue.Count > 0)
        {
            isSaving = true;

            // Dequeue and execute the next save operation
            saveQueue.Dequeue()();

            // Wait until the end of frame before executing the next save operation
            // to ensure the previous save operation has enough time to complete
            yield return new WaitForEndOfFrame();
        }
    }

    public void LoadData(bool undo)
    {
        
        // If undoing
        if (undo)
        {
           
            // Only decrement currentDataIndex if it's greater than 0 and no loading is in progress
            if (currentDataIndex > 0 && !isLoadingData)
            {
                currentDataIndex--;
                StartCoroutine(LoadDataCoroutine());
                
            }
        }
        // If redoing
        else
        {
            
            // Only increment currentDataIndex if it's less than the total number of saved files - 1 and no loading is in progress
            if (currentDataIndex < savedFiles.Count - 1 && !isLoadingData)
            {
                
                currentDataIndex++;
                StartCoroutine(LoadDataCoroutine());
                
            }
        }
       
    }


    private IEnumerator LoadDataCoroutine()
    {
        isLoadingData = true;

        if (startPart != null)
        {
            Destroy(startPart);
        }

        // Wait until the end of the frame so that the startPart is fully destroyed
        yield return new WaitForEndOfFrame();

        var settings = new ES3Settings();
        settings.location = ES3.Location.Cache;

        if (currentDataIndex < savedFiles.Count) // Ensure the index is within the range of savedFiles
        {
           
            ES3.Load<List<GameObject>>("startPart", savedFiles[currentDataIndex], settings);
            startPart = GameObject.Find("StartPart");
            bounds.targetObject = startPart;
            bounds.CalculateBounds();
            indicator.MoveToLastSnapPoint();
        }

        isLoadingData = false;
        
        
    }
    public void SaveSlideAsSample()
    {
        var sampleSettings = new ES3Settings();
        sampleSettings.location = ES3.Location.File;
        ES3.Save<GameObject>("myGameObject", startPart, "sample10.es3", sampleSettings);
        byte[] sampleBytes = ES3.LoadRawBytes("sample10.es3", sampleSettings);
        ES3.SaveRaw(sampleBytes,"sample10.bytes", sampleSettings);
        Debug.Log("Slide saved");
    }

    public void SaveJSON()
    {
        rotator.DeselectObject();
        string fileName = filenameInput.text;
        // Save the GameObject as a base64 string.
        string base64GameObject = SaveToBase64();

        // Save the screenshot as a base64 string.
        string base64Screenshot = SaveRenderTextureToBase64();

        // Start the coroutine to upload the data.
        StartCoroutine(UploadData(base64GameObject, base64Screenshot, fileName));
    }
    public void SaveJSONNoEmail()
    {
        rotator.DeselectObject();
        string fileName = filenameInput.text;
        // Save the GameObject as a base64 string.
        string base64GameObject = SaveToBase64();

        // Save the screenshot as a base64 string.
        string base64Screenshot = SaveRenderTextureToBase64();

        // Start the coroutine to upload the data.
        StartCoroutine(UploadDataNoEmail(base64GameObject, base64Screenshot, fileName));
    }
    public void SaveJSONReplace()
    {
        rotator.DeselectObject();
        string objectId = loadController.loadedSlideId.ToString();
        // Save the GameObject as a base64 string.
        if (objectId != null)
        {
            string fileName = filenameInput.text;
            string base64GameObject = SaveToBase64();

            // Save the screenshot as a base64 string.
            string base64Screenshot = SaveRenderTextureToBase64();

            // Start the coroutine to upload the data.
            StartCoroutine(ReplaceData(objectId, base64GameObject, base64Screenshot, fileName));
        }
    }

    private List<GameObject> GatherTaggedPrefabs(GameObject root)
    {
        List<GameObject> taggedPrefabs = new List<GameObject>();

        // Check the root GameObject itself if it has the desired tags
        if (root.tag == "Curved" || root.tag == "Straight" || root.tag == "Beam")
        {
            taggedPrefabs.Add(root);
        }

        // Check all the children of the root
        foreach (Transform child in root.transform)
        {
           
                taggedPrefabs.Add(child.gameObject);
            

            // recursively check the children of this child
            taggedPrefabs.AddRange(GatherTaggedPrefabs(child.gameObject));
        }

        return taggedPrefabs;
    }

    public string SaveToBase64()
    {
        var cacheSettings = new ES3Settings(ES3.Location.Cache);

       
        List<GameObject> taggedPrefabs = GatherTaggedPrefabs(startPart);
        Debug.Log(taggedPrefabs);
        
        ES3.Save<List<GameObject>>("myGameObjects", taggedPrefabs, cacheSettings);
        
        //string base64 = ES3.LoadRawString(cacheSettings)
        byte[] bytes = ES3.LoadRawBytes(cacheSettings);

        string base64 = Convert.ToBase64String(bytes);

        Debug.Log("Base64" + base64);

        return base64;
    }
    public string SaveRenderTextureToBase64()
    {
        axoCamera.enabled = true;
        RenderTexture renderTexture = axoCamera.targetTexture;
        byte[] bytes = RenderTextureToBytes(renderTexture);
        string base64 = Convert.ToBase64String(bytes);
        Debug.Log(base64);
        return base64;
    }

    public byte[] RenderTextureToBytes(RenderTexture renderTexture)
    {
        // Create a new Texture2D and set its pixel values to the RenderTexture
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        // Then convert to bytes (e.g., PNG)
        byte[] pngBytes = texture2D.EncodeToPNG();

        // Cleanup: free memory used by the Texture2D
        Destroy(texture2D);
        return pngBytes;
    }

    IEnumerator UploadDataNoEmail(string base64GameObject, string base64Screenshot, string fileName)
    {
        string localPrice = generatePricingList.totalPrice.ToString() + " " + generatePricingList.currency;
        WWWForm form = new WWWForm();
        form.AddField("json", base64GameObject); // Pass the base64 string of the GameObject
        form.AddField("screenshot", base64Screenshot); // Pass the base64 string of the screenshot
        form.AddField("username", uiManager.accountUsername);
        form.AddField("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        form.AddField("price", localPrice);
        form.AddField("filename", fileName);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/upload_data.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            warningTextSave.enabled = true;
            warningTextSave.text = "Upload complete!";
            savePanel.SetActive(false);
            Debug.Log("Upload complete!");
        }
    }

    IEnumerator UploadData(string base64GameObject, string base64Screenshot, string fileName)
    {
        string localPrice = generatePricingList.totalPrice.ToString() + " " + generatePricingList.currency;
       
        WWWForm form = new WWWForm();
        form.AddField("json", base64GameObject); // Pass the base64 string of the GameObject
        form.AddField("screenshot", base64Screenshot); // Pass the base64 string of the screenshot
        form.AddField("username", uiManager.accountUsername);
        form.AddField("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        form.AddField("price", localPrice);
        form.AddField("filename", fileName);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/upload_data.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            IEnumerator DeactivateAfterSeconds(GameObject panel, float seconds)
            {
                yield return new WaitForSeconds(seconds);
                panel.SetActive(false);
            }
            warningTextSave.enabled = true;
            warningTextSave.text = "Upload complete!";
            savePanel.SetActive(false);
            notificationPanel.SetActive(true);
            StartCoroutine(DeactivateAfterSeconds(notificationPanel, 4));
            Debug.Log("Upload complete!");
            string objectId = www.downloadHandler.text.Trim();  // Assuming the server response is the ID

            StartCoroutine(SendEmailNotification(objectId));
        }
    }
    IEnumerator ReplaceData(string objectId, string base64GameObject, string base64Screenshot, string fileName)
    {
        string localPrice = generatePricingList.totalPrice.ToString() + " " + generatePricingList.currency;
        WWWForm form = new WWWForm();
        form.AddField("id", objectId); // Pass the ID that you want to replace
        form.AddField("json", base64GameObject); // Pass the new base64 string of the GameObject
        form.AddField("screenshot", base64Screenshot); // Pass the new base64 string of the screenshot
        form.AddField("username", uiManager.accountUsername);
        form.AddField("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        form.AddField("price", localPrice);
        form.AddField("filename", fileName);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/replace_data.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            warningTextSave.enabled = true;
            warningTextSave.text = "Data replacement complete!";
            savePanel.SetActive(false);
            Debug.Log("Data replacement complete!");
        }
    }


    public IEnumerator SendEmailNotification(string objectId)
    {
        WWWForm form = new WWWForm();
        form.AddField("objectId", objectId);
        form.AddField("username", uiManager.accountUsername);
        form.AddField("email", uiManager.accountEmail);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/send_email.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Email sent successfully.");
        }
    }
    private void FixedUpdate()
    {
        if(startPart == null)
        {
            startPart = GameObject.Find("StartPart");
        }
    }
}