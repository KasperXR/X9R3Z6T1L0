using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SavePanel : MonoBehaviour
{
    public GameObject savePanel;
    public TMP_InputField filename;
    public TMP_Text warningText;
    public GameObject commandManager;

    public void SaveSlide()
    {
        if (string.IsNullOrEmpty(filename.text))
        {
            warningText.text = "Enter filename to save!";
            warningText.gameObject.SetActive(true);
        }
        else
        {
            warningText.gameObject.SetActive(false);
            StartPartController startPart = commandManager.GetComponent<StartPartController>();
            startPart.filename = filename.text;
            startPart.SaveJSONNoEmail();
            warningText.text = "Slide has been saved!";
            warningText.gameObject.SetActive(true);
        }
    }
}