using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldSubmit : MonoBehaviour
{
    public TMP_InputField inputField;  // The input field
    public SpawnStart spawnStart;
    public bool isFocused = false;

    void Update()
    {
        // check if the input field is selected and "Return" or "Enter" was pressed this frame
        if (isFocused && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            SubmitInputField();
        }
    }

    void SubmitInputField()
    {
        Debug.Log("SubmitInputField called");
        spawnStart.SpawnStartObject();
    }
    public void InputFocused()
    {
        isFocused = true;
    }
    public void InputnotFocused()
    {
        isFocused = false;
    }
}