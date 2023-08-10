using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    private Stack<string> undoStack = new Stack<string>();
    private Stack<string> redoStack = new Stack<string>();
    public GameObject startPart;

    // Create a string to hold the path to your file
    private string filePath;

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "tempundo.es3");
    }

    public void Update()
    {
        if (startPart == null)
        {
            startPart = GameObject.Find("StartPart");
        }
    }

    public void SaveStateForUndo()
    {
        try
        {
            // Save the current state of the game object to a temporary file.
            ES3.Save<GameObject>("myGameObject", startPart, new ES3Settings { location = ES3.Location.File, path = filePath });

            // Read the contents of the file into a string.
            if (File.Exists(filePath)) // Check if the file exists before trying to read it
            {
                string state = File.ReadAllText(filePath);

                // Push this state onto the undo stack.
                undoStack.Push(state);
            }
            else
            {
                Debug.LogError("File " + filePath + " does not exist!");
            }

            // Clear the redo stack whenever we save a new state for undo.
            redoStack.Clear();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception in SaveStateForUndo: " + e.Message);
        }
    }


    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            // Save the current state for redo.
            ES3.Save<GameObject>("myGameObject", startPart, new ES3Settings { location = ES3.Location.File, path = filePath });
            string state = File.ReadAllText(filePath);
            redoStack.Push(state);

            // Pop the last state from the undo stack and write it to a file.
            state = undoStack.Pop();
            File.WriteAllText(filePath, state);

            // Load the state from the file.
            startPart = ES3.Load<GameObject>("myGameObject", new ES3Settings { location = ES3.Location.File, path = filePath });
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            // Save the current state for undo.
            ES3.Save<GameObject>("myGameObject", startPart, new ES3Settings { location = ES3.Location.File, path = filePath });
            string state = File.ReadAllText(filePath);
            undoStack.Push(state);

            // Pop the last state from the redo stack and write it to a file.
            state = redoStack.Pop();
            File.WriteAllText(filePath, state);

            // Load the state from the file.
            startPart = ES3.Load<GameObject>("myGameObject", new ES3Settings { location = ES3.Location.File, path = filePath });
        }
    }
}
