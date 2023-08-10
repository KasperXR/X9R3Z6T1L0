using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    private HierarchyBounds bounds;
    private RulesChecker checker;

    private void Start()
    {
        bounds = GameObject.Find("CommandManager").GetComponent<HierarchyBounds>();
        checker = GameObject.Find("CommandManager").GetComponent<RulesChecker>();
    }

    public void DestroyStartPart()
    {
        // Destroy the startPart and reset values
        GameObject startPart = GameObject.Find("StartPart");
        if (startPart != null)
        {
            Destroy(startPart);
            bounds.lengthText.text = "";
            bounds.widthText.text = "";
            bounds.boundsCube.transform.localScale = new Vector3(0, 0, 0);
            checker.distanceText.text = "Length: " + "0" + "cm" + "  " + "Avg slope: " + "0" + "°";
        }
    }

    public void ResetScene()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
