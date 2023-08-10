using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq; // Add this line

public class UIManager : MonoBehaviour
{
    public TMP_InputField loginUsernameField;
    public TMP_InputField loginPasswordField;
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;
    public TMP_InputField emailField;

    public GameObject adminPanel;
    public GameObject userEntryPrefab;
    public Transform userEntryContainer;
    public GameObject loginPanel;
    public GameObject loginPart;
    public GameObject registerPart;
    public Button nextPageButton;
    public Button previousPageButton;
    public Transform userListPanel;
    public TMP_Text usernameText;
    public TMP_Text emailText;
    public TMP_Text accountTypeText;
    public string accountUsername = null;
    public string accountType = null;
    public string accountEmail = null;
    public string accountLocation = "DKK";
    public TMP_InputField currentPasswordField;
    public TMP_InputField newPasswordField;
    public GameObject saveButton;
    public GameObject exportCSVButton;
    public GameObject savePNGButton;
    public GameObject requestButton;
    public GameObject loginWarning;
    public GameObject searchFunction;
    public GeneratePricingList priceList;

    

    public int entriesPerPage = 10; // Set the number of entries displayed per page

    private int currentPage = 0; // Keep track of the current page
    private List<string> accountTypes = new List<string> { "guest", "verified", "admin" };
    List<string> locations = new List<string>() { "DK", "Export" };

    [Serializable]
    public class UserData
    {
        public string username;
        public string email;
        public string account_type;
        public string location;
    }

    [System.Serializable]
    public class UserDataList
    {
        public List<UserData> users;
    }
    [Serializable]
    public class UserAdminData
    {
        public string username;
        public string account_type;
        public string location;
    }

    [Serializable]
    public class UserAdminDataList
    {
        public List<UserAdminData> users;
    }
    public void Register()
    {
        StartCoroutine(RegisterUser());
    }

    public void Login()
    {
        StartCoroutine(LoginUser());
    }

    public void FetchUserData()
    {
        StartCoroutine(FetchUsers());
    }

    public void UpdateAccountType(string username, string newAccountType)
    {
        StartCoroutine(UpdateUserAccountType(username, newAccountType));
    }
   

    IEnumerator RegisterUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", registerUsernameField.text);
        form.AddField("password", registerPasswordField.text);
        form.AddField("email", emailField.text);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/register.php", form);

        // Check for internet connectivity
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connection available");
            yield break; // Exit the coroutine
        }


        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            registerPart.SetActive(false);
            loginPart.SetActive(true);
            

        }
    }


    IEnumerator LoginUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", loginUsernameField.text);
        form.AddField("password", loginPasswordField.text);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/login.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            loginWarning.SetActive(true);
            Debug.Log(www.error);
            

        }
        else
        {
            UserData userData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
            usernameText.text = "Account: " + userData.username;
            emailText.text = "E-mail: " + userData.email;
            accountTypeText.text = "Account Type: " + userData.account_type;
            accountUsername = userData.username;
            accountEmail = userData.email;
            accountType = userData.account_type;
            accountLocation = userData.location; // Save user location

            SetPermissions(userData.account_type);
            loginWarning.SetActive(false);
            loginPanel.SetActive(false);
            StartCoroutine(priceList.FetchAndSetupPrices());
            StartCoroutine(priceList.UpdateListCoroutine());
        }
    }


    IEnumerator FetchUsers()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://xplorxr.dk/LEDON/Scripts/fetch_users.php");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("JSON Data: " + www.downloadHandler.text);
            PopulateAdminPanel(www.downloadHandler.text);
        }
    }

    IEnumerator UpdateUserAccountType(string username, string newAccountType)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("new_account_type", newAccountType);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/update_account_type.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
    }
    IEnumerator UpdateUserLocation(string username, string newLocation)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("new_location", newLocation);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/update_user_location.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
    }

    private void PopulateAdminPanel(string jsonData)
    {
        // Deserialize JSON data
        UserAdminDataList userList = JsonUtility.FromJson<UserAdminDataList>(jsonData);

        // Check if userList is null or userList.users is null
        if (userList == null || userList.users == null)
        {
            Debug.LogError("Error: userList or userList.users is null.");
            return;
        }

        // Calculate the range of entries to display on the current page
        int startIndex = currentPage * entriesPerPage;
        int endIndex = Mathf.Min(startIndex + entriesPerPage, userList.users.Count);

        // Remove all existing user entries from the container
        foreach (Transform child in userEntryContainer)
        {
            Destroy(child.gameObject);
        }

        // Create user entries based on the deserialized data and the current page
        for (int i = startIndex; i < endIndex; i++)
        {
            UserAdminData user = userList.users[i];
            GameObject userEntry = Instantiate(userEntryPrefab, userEntryContainer);
            TMP_Text usernameText = userEntry.transform.Find("UsernameText").GetComponent<TMP_Text>();

            // Find the TMP_Dropdown component in the user entry for Account Type
            TMP_Dropdown accountTypeDropdown = userEntry.transform.Find("AccountTypeDropdown").GetComponent<TMP_Dropdown>();

            // Set the Dropdown options for Account Type
            accountTypeDropdown.ClearOptions();
            accountTypeDropdown.AddOptions(accountTypes);

            // Set the selected option in the Dropdown based on the user's account type
            accountTypeDropdown.value = accountTypes.IndexOf(user.account_type);

            // Assign the listener for the Dropdown's onValueChanged event
            accountTypeDropdown.onValueChanged.AddListener(delegate (int index) { UpdateAccountType(user.username, accountTypes[index]); });

            // Find the TMP_Dropdown component in the user entry for Location
            TMP_Dropdown locationDropdown = userEntry.transform.Find("LocationDropdown").GetComponent<TMP_Dropdown>();

            // Set the Dropdown options for Location
            locationDropdown.ClearOptions();
            locationDropdown.AddOptions(locations);

            // Set the selected option in the Dropdown based on the user's location
            locationDropdown.value = locations.IndexOf(user.location);

            // Assign the listener for the Dropdown's onValueChanged event
            locationDropdown.onValueChanged.AddListener(delegate (int index) { StartCoroutine(UpdateUserLocation(user.username, locations[index])); });

            if (usernameText != null)
            {
                usernameText.text = user.username;
            }
            else
            {
                Debug.LogError("Error: UsernameText is not found.");
            }
        }
    }

    public void ChangePassword()
    {
        StartCoroutine(ChangePasswordCoroutine());
    }
    IEnumerator ChangePasswordCoroutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", loginUsernameField.text);
        form.AddField("current_password", currentPasswordField.text);
        form.AddField("new_password", newPasswordField.text);

        UnityWebRequest www = UnityWebRequest.Post("https://xplorxr.dk/LEDON/Scripts/change_password.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
    }

    void SetPermissions(string accountType)
    {
        // Set permissions based on account type
        switch (accountType)
        {
            case "admin":
                // Set admin permissions
                
                exportCSVButton.SetActive(true);
                savePNGButton.SetActive(true);
                saveButton.GetComponent<Button>().interactable = true;
                exportCSVButton.GetComponent<Button>().interactable = true;
                savePNGButton.GetComponent<Button>().interactable = true;
                searchFunction.SetActive(true);
                break;
            case "verified":
                // Set verified user permissions
                adminPanel.SetActive(false);
                saveButton.GetComponent<Button>().interactable = true;
                exportCSVButton.SetActive(false);
                savePNGButton.SetActive(false);
                requestButton.SetActive(true);
                searchFunction.SetActive(false);
                // Add additional permissions for verified users here
                break;
            case "guest":
                // Set guest permissions
                adminPanel.SetActive(false);
                saveButton.GetComponent<Button>().interactable = false;
                exportCSVButton.GetComponent<Button>().interactable = false;
                savePNGButton.GetComponent<Button>().interactable = false;
                exportCSVButton.SetActive(false);
                savePNGButton.SetActive(false);
                searchFunction.SetActive(false);
                // Add additional permissions for guest users here
                break;
            default:
                // Handle unknown account type
                saveButton.SetActive(false);
                adminPanel.SetActive(false);
                saveButton.GetComponent<Button>().interactable = false;
                exportCSVButton.GetComponent<Button>().interactable = false;
                savePNGButton.GetComponent<Button>().interactable = false;
                exportCSVButton.SetActive(false);
                savePNGButton.SetActive(false);
                searchFunction.SetActive(false);
                break;
        }
    }

    public void NextPage()
    {
        currentPage++;
        FetchUserData();
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            FetchUserData();
        }
    }
    private void Start()
    {
        
    }
}



