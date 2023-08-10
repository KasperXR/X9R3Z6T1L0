using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ListObject : MonoBehaviour
{
    public TextMeshProUGUI idText; // Assign this in the Inspector
    public TextMeshProUGUI priceText; // Assign this in the Inspector
    public TextMeshProUGUI usernameText; // Assign this in the Inspector
    public RawImage objectImage; // Assign this in the Inspector

    private int id;
    private string filename;  // Add filename field here

    public GameObject confirmationDialog;
    public Button yesButton;
    public Button noButton;

    public void SetData(int id, string price, string username, string imageBase64, string filename = null)  // Add filename parameter here
    {
        this.id = id;
        this.filename = filename;  // Set filename here

        // If filename is not null, display filename, else display id
        idText.text = string.IsNullOrEmpty(filename) ? "ID: " + id : filename;

        priceText.text = "Price: " + price;
        usernameText.text = "Created: " + username;

        Texture2D imageTexture = Base64ToTexture(imageBase64);
        if (imageTexture != null)
        {
            objectImage.texture = imageTexture;
        }
    }

    Texture2D Base64ToTexture(string base64)
    {
        byte[] imageData = Convert.FromBase64String(base64);

        Texture2D tex = new Texture2D(360, 278);
        if (tex.LoadImage(imageData))
        {
            return tex;
        }
        else
        {
            Debug.LogError("Could not load image from base64!");
            return null;
        }
    }

    public void LoadGameObject()
    {
        // Remove old listeners
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        // Set the id of the object to load when showing the confirmation dialog
        yesButton.onClick.AddListener(ConfirmLoadGameObject);
        noButton.onClick.AddListener(CancelLoadGameObject);

        // Show the confirmation dialog
        confirmationDialog.SetActive(true);
    }

    public void ConfirmLoadGameObject()
    {
        // Load the game object
        LoadGameObjectController.Instance.LoadGameObjectById(id);

        // Hide the confirmation dialog
        confirmationDialog.SetActive(false);
    }

    public void CancelLoadGameObject()
    {
        // Hide the confirmation dialog
        confirmationDialog.SetActive(false);
    }
}
