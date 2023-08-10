using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadSampleSlide : MonoBehaviour
{
    public string filePath;
    public GameObject confirmationDialog;
    public Button yesButton;
    public Button noButton;
    private GameObject startPart;
    private StartPartController controller;
    public HierarchyBounds bounds;

    private void Start()
    {
        controller = GameObject.Find("CommandManager").GetComponent<StartPartController>();
        bounds = GameObject.Find("CommandManager").GetComponent<HierarchyBounds>();
    }

    public void ShowConfirmationDialog()
    {
        // Add the listeners when the confirmation dialog is shown
        yesButton.onClick.AddListener(ContinueLoading);
        noButton.onClick.AddListener(CloseConfirmationDialog);

        confirmationDialog.SetActive(true);
    }

    public void CloseConfirmationDialog()
    {
        // Remove the listeners when the confirmation dialog is closed
        yesButton.onClick.RemoveListener(ContinueLoading);
        noButton.onClick.RemoveListener(CloseConfirmationDialog);

        confirmationDialog.SetActive(false);
    }

    public void LoadSlideSample()
    {
        ShowConfirmationDialog();
    }

    public void ContinueLoading()
    {
        if(startPart == null)
        {
            startPart = GameObject.Find("StartPart");
            Destroy(startPart);
        }
        var settings = new ES3Settings();
        settings.location = ES3.Location.Resources;
        ES3.Load("myGameObject", "Samples/" + filePath, settings);
        StartCoroutine(CalculateBounds());

        CloseConfirmationDialog(); // This will also remove the listeners
    }

    IEnumerator CalculateBounds()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Bounds calculated");
        bounds.CalculateBounds();
        if (controller.startPart == null)
        {
            controller.startPart = GameObject.Find("StartPart");
        }
    }
}