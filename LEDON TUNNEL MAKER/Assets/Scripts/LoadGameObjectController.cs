using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.IO;

[System.Serializable]
public class GameObjectEntry
{
    public int id;
    public string jsonData;
    public string username;
    public string save_date;
    public string price;
    public string imageBase64;
    public string filename;

    public override string ToString()
    {
        string displayID = string.IsNullOrEmpty(filename) ? id.ToString() : filename;  // Modify this line
        return $"ID: {displayID}, Serialized Data: {jsonData}, Save Date: {save_date}, Username: {username}, Price: {price}";
    }
}

[System.Serializable]
public class GameObjectList
{
    public List<GameObjectEntry> game_object_entries;
}

public class LoadGameObjectController : MonoBehaviour
{
    public static LoadGameObjectController Instance;

    public ListObject[] listObjects;
    public Button nextPageButton;
    public Button previousPageButton;
    public Button searchButton; // Added search button
    public TMP_InputField searchInputField; // Added input field
    public int currentPage = 0;
    public const int entriesPerPage = 8;
    public StartPartController startPartController;
    public RulesChecker checker;
    public SpawnStart spawnStart;
    public GameObject loadingPanel;
    private HierarchyBounds bounds;
    private UIManager uiManager;
    public GeneratePricingList generatePricingList;
    public TMP_Dropdown searchTypeDropdown;
    private Indicator indicator;
    public int loadedSlideId;
    public TextMeshProUGUI infotext;
    public Button replaceSavingButton;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        bounds = transform.GetComponent<HierarchyBounds>();
        searchButton.onClick.AddListener(SearchButtonClicked); // Listener for search button click
        searchTypeDropdown.onValueChanged.AddListener(delegate {
            SearchTypeDropdownValueChanged(searchTypeDropdown);
        });
        uiManager = GameObject.Find("CommandManager").GetComponent<UIManager>();
        indicator = GameObject.Find("Indicator").GetComponent<Indicator>();
        

    }

    private void SearchButtonClicked()
    {
        string searchType = searchTypeDropdown.options[searchTypeDropdown.value].text;

        if (searchType == "ID")
        {
            int id;
            if (int.TryParse(searchInputField.text, out id))
            {
                GetSingleGameObject(id);
            }
            else
            {
                Debug.LogError("Invalid ID entered for search.");
            }
        }
        else if (searchType == "Username")
        {
            string username = searchInputField.text;

            if (!string.IsNullOrWhiteSpace(username))
            {
                GetSingleGameObjectByUsername(username);
            }
            else
            {
                Debug.LogError("Invalid username entered for search.");
            }
        }
        else if (searchType == "Filename") // Filename search
        {
            string filename = searchInputField.text;

            if (!string.IsNullOrWhiteSpace(filename))
            {
                GetSingleGameObjectByFilename(filename);
            }
            else
            {
                Debug.LogError("Invalid filename entered for search.");
            }
        }
    }
    void SearchTypeDropdownValueChanged(TMP_Dropdown change)
    {
        Debug.Log("Search type changed to: " + change.options[change.value].text);
    }
    public void LoadNextPage()
    {
        currentPage++;
        if (uiManager.accountType == "admin")
        {
            StartCoroutine(GetGameObjects());
        }
        else if (uiManager.accountType == "verified")
        {
            StartCoroutine(GetGameObjectsForUser());
        }
    }

    public void LoadPreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            if (uiManager.accountType == "admin")
            {
                StartCoroutine(GetGameObjects());
            }
            else if (uiManager.accountType == "verified")
            {
                StartCoroutine(GetGameObjectsForUser());
            }
        }
    }

    public void GetGameObjectButton()
    {
        if(uiManager.accountType == "admin")
        {
            StartCoroutine(GetGameObjects());
        }
        else if(uiManager.accountType == "verified")
        {
            StartCoroutine(GetGameObjectsForUser());
        }
        else
        {
            return;
        }
        
    }
    public void GetSingleGameObjectByFilename(string filename)
    {
        StartCoroutine(GetGameObjectByFilenameCoroutine(filename));
    }

    public IEnumerator GetGameObjectByFilenameCoroutine(string filename)
    {
        WWWForm form = new WWWForm();
        form.AddField("filename", filename);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/get_game_objects.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("UnityWebRequest Error: " + www.error);
        }
        else
        {
            Debug.Log("UnityWebRequest Success!");
            string jsonString = www.downloadHandler.text;
            Debug.Log("Received JSON String: " + jsonString);

            PopulateList(jsonString);
        }
    }
    public void GetSingleGameObjectByUsername(string username)
    {
        StartCoroutine(GetGameObjectByUsernameCoroutine(username));
    }
    public IEnumerator GetGameObjectByUsernameCoroutine(string username)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/get_game_objects.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("UnityWebRequest Error: " + www.error);
        }
        else
        {
            Debug.Log("UnityWebRequest Success!");
            string jsonString = www.downloadHandler.text;
            Debug.Log("Received JSON String: " + jsonString);

            PopulateList(jsonString);
        }
    }

    IEnumerator GetGameObjects()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://xplorxr.dk/LEDON/Scripts/get_game_objects.php");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string jsonString = www.downloadHandler.text;
            PopulateList(jsonString);
            Debug.Log($"Raw JSON response: {jsonString}");
        }
    }
    IEnumerator GetGameObjectsForUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", uiManager.accountUsername);
        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/get_game_objects_for_user.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string jsonString = www.downloadHandler.text;
            PopulateList(jsonString);
            Debug.Log($"Raw JSON response: {jsonString}");
        }
    }

    public void GetSingleGameObject(int id)
    {
        StartCoroutine(GetSingleGameObjectCoroutine(id));
    }

    public IEnumerator GetSingleGameObjectCoroutine(int id)
    {
        Debug.Log("Button Clicked");
        WWWForm form = new WWWForm();
        form.AddField("id", id);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/get_game_objects.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("UnityWebRequest Error: " + www.error);
        }
        else
        {
            Debug.Log("UnityWebRequest Success!");
            string jsonString = www.downloadHandler.text;
            Debug.Log("Received JSON String: " + jsonString);

            PopulateListSingle(jsonString);
        }
    }

    void PopulateList(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
        {
            Debug.LogError("Received an empty or null JSON string.");
            return;
        }

        List<GameObjectEntry> gameObjectEntries = JsonHelper.FromJson<GameObjectEntry>(jsonString);
        Debug.Log($"Deserialized {gameObjectEntries.Count} entries.");
        if (gameObjectEntries == null)
        {
            Debug.LogError("Failed to deserialize JSON data.");
            return;
        }

        foreach (GameObjectEntry entry in gameObjectEntries)
        {
            Debug.Log(entry.ToString());
        }

        int startIndex = currentPage * entriesPerPage;
        Debug.Log($"Current Page: {currentPage}. Start Index: {startIndex}");

        // Reset all ListObjects
        foreach (ListObject listObject in listObjects)
        {
            listObject.gameObject.SetActive(false);
        }

        for (int i = startIndex; i < startIndex + entriesPerPage && i - startIndex < listObjects.Length; i++)
        {
            if (i < gameObjectEntries.Count)
            {
                GameObjectEntry entry = gameObjectEntries[i];

                listObjects[i - startIndex].SetData(entry.id, entry.price, entry.username, entry.imageBase64, entry.filename);  
                Debug.Log($"Setting data for ListObject[{i - startIndex}]: {entry.ToString()}");
                listObjects[i - startIndex].gameObject.SetActive(true);
            }
            else
            {
                listObjects[i - startIndex].gameObject.SetActive(false);
            }
        }

        nextPageButton.interactable = gameObjectEntries.Count > (currentPage + 1) * entriesPerPage;
        Debug.Log($"Next Page Button Interactable: {nextPageButton.interactable}");

        previousPageButton.interactable = currentPage > 0;
        Debug.Log($"Previous Page Button Interactable: {previousPageButton.interactable}");
    }
    void PopulateListSingle(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
        {
            Debug.LogError("Received an empty or null JSON string.");
            return;
        }

        GameObjectEntry gameObjectEntry = JsonUtility.FromJson<GameObjectEntry>(jsonString);
        Debug.Log($"Deserialized entry: {gameObjectEntry.ToString()}");
        if (gameObjectEntry == null)
        {
            Debug.LogError("Failed to deserialize JSON data.");
            return;
        }

        // Reset all ListObjects
        foreach (ListObject listObject in listObjects)
        {
            listObject.gameObject.SetActive(false);
        }

        listObjects[0].SetData(gameObjectEntry.id, gameObjectEntry.price, gameObjectEntry.username, gameObjectEntry.imageBase64, gameObjectEntry.filename);
        Debug.Log($"Setting data for ListObject[0]: {gameObjectEntry.ToString()}");
        listObjects[0].gameObject.SetActive(true);

        for (int i = 1; i < listObjects.Length; i++)
        {
            listObjects[i].gameObject.SetActive(false);
        }

        nextPageButton.interactable = false;
        Debug.Log($"Next Page Button Interactable: {nextPageButton.interactable}");

        previousPageButton.interactable = currentPage > 0;
        Debug.Log($"Previous Page Button Interactable: {previousPageButton.interactable}");
    }


    public void LoadGameObjectById(int id)
    {
        StartCoroutine(LoadGameObjectCoroutine(id));
    }

    public IEnumerator LoadGameObjectCoroutine(int id)
    {
        Debug.Log("Button Clicked");
        WWWForm form = new WWWForm();
        form.AddField("id", id);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/get_game_objects.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("UnityWebRequest Error: " + www.error);
        }
        else
        {
            Debug.Log("UnityWebRequest Success!");
            string jsonString = www.downloadHandler.text;
            Debug.Log("Received JSON String: " + jsonString);

            GameObjectEntry entry = JsonUtility.FromJson<GameObjectEntry>(jsonString);
            Debug.Log("Works: "+ entry.ToString());

            try
            {

                byte[] data = Convert.FromBase64String(entry.jsonData);

                if (startPartController.startPart != null)
                {
                    Destroy(startPartController.startPart);
                }

                Debug.Log("Saving bytes to the cache using ES3...");
                var cacheSettings = new ES3Settings(ES3.Location.Cache);
                ES3.SaveRaw(data, cacheSettings);

                Debug.Log("Loading GameObject from cache...");
                ES3.Load<List<GameObject>>("myGameObjects", cacheSettings);
                if (entry.price.Contains("EUR") && uiManager.accountType == "admin")
                {
                    uiManager.accountLocation = "Export";
                }
                else if (entry.price.Contains("DKK") && uiManager.accountType == "admin")
                {
                    uiManager.accountLocation = "DK";
                }
                loadedSlideId = entry.id;
                
                checker.CheckDistanceandSlope();
                StartCoroutine(CalculateBounds());
                
                loadingPanel.SetActive(false);
                spawnStart.existingPrefab = checker.startPart = GameObject.Find("StartPart");
                startPartController.startPart = checker.startPart;
                spawnStart.meterValue.text = ((startPartController.startPart.transform.position.y - 0.3805935f)*100).ToString();
                StartCoroutine(IndicatorMoveDelayed());
                infotext.text = "You have modified loaded slide with ID: " + loadedSlideId +", you can save it as new or replace the old one.";
                replaceSavingButton.interactable = true;





            }
            catch (System.Exception e)
            {
                Debug.LogError("An error occurred: " + e.Message);
            }
        }
    }
    IEnumerator IndicatorMoveDelayed()
    {
        yield return new WaitForSeconds(0.5f); // Wait for 1 second
        indicator.MoveToLastSnapPoint();
    }
    IEnumerator CalculateBounds()
    {
        yield return new WaitForSeconds(0.5f);
        bounds.CalculateBounds();
    }
}
