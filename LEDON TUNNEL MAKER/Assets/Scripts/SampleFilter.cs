using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SampleFilter : MonoBehaviour
{
    // Your input fields
    // Your input fields
    public TMP_InputField heightInputField;
    public TMP_InputField widthInputField;
    public TMP_InputField lengthInputField;

    // Your scroll view's content
    public Transform scrollViewContent;

    // Raw image buttons in the scroll view
    private List<Button> rawImageButtons;

    private void Start()
    {
        rawImageButtons = new List<Button>(scrollViewContent.GetComponentsInChildren<Button>());

        // Set the OnValueChanged listeners for the input fields
        heightInputField.onValueChanged.AddListener(delegate { FilterImages(); });
        widthInputField.onValueChanged.AddListener(delegate { FilterImages(); });
        lengthInputField.onValueChanged.AddListener(delegate { FilterImages(); });
    }

    private void FilterImages()
    {
        // Get the user input
        int inputHeight = string.IsNullOrEmpty(heightInputField.text) ? int.MaxValue : int.Parse(heightInputField.text);
        int inputWidth = string.IsNullOrEmpty(widthInputField.text) ? int.MaxValue : int.Parse(widthInputField.text);
        int inputLength = string.IsNullOrEmpty(lengthInputField.text) ? int.MaxValue : int.Parse(lengthInputField.text);

        foreach (Button button in rawImageButtons)
        {
            // Extract the dimensions from the button name
            string[] dimensions = button.name.Split('x');
            int buttonHeight = int.Parse(dimensions[0]);
            int buttonLength = int.Parse(dimensions[1]);
            int buttonWidth = int.Parse(dimensions[2]);

            // Check if the button dimensions are less than or equal to the input
            if (buttonHeight <= inputHeight && buttonLength <= inputLength && buttonWidth <= inputWidth)
            {
                button.gameObject.SetActive(true);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
    }
}