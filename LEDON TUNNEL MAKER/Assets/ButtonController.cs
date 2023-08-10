using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public GameObject initializeButton;
    public Button environmentButton;
    public Button designButton;
    public Button exportButton;
    public Button sampleButton;  // Declare the new 5th button here
    public GameObject initializeButtonContent;
    private bool isMoving = false;
    public float moveDuration = 0.5f;

    private RectTransform[] buttonParents;
    private Vector3[] initialPositions;
    private int activeButtonIndex = -1;

    private GameObject[] buttonArrows;

    private void Start()
    {
        buttonParents = new RectTransform[] {
            initializeButton.transform.parent.GetComponent<RectTransform>(),
            environmentButton.transform.parent.GetComponent<RectTransform>(),
            designButton.transform.parent.GetComponent<RectTransform>(),
            sampleButton.transform.parent.GetComponent<RectTransform>(),
            exportButton.transform.parent.GetComponent<RectTransform>()
            
        };

        initialPositions = new Vector3[buttonParents.Length];
        for (int i = 0; i < buttonParents.Length; i++)
        {
            initialPositions[i] = buttonParents[i].anchoredPosition;
        }

        buttonArrows = new GameObject[] {
            initializeButton.transform.parent.Find("Arrow").gameObject,
            environmentButton.transform.parent.Find("Arrow").gameObject,
            designButton.transform.parent.Find("Arrow").gameObject,
            sampleButton.transform.parent.Find("Arrow").gameObject,
            exportButton.transform.parent.Find("Arrow").gameObject
             // Include the new button
        };
    }

    public IEnumerator MoveButtons(int buttonIndex)
    {
        isMoving = true;
        Vector3[] startPositions = new Vector3[buttonParents.Length];
        Vector3[] targetPositions = new Vector3[buttonParents.Length];

        float[] targetYPositions = new float[] { -93.94f, -169.3f, -245.3f, -321f, -394.8f };

        for (int i = 0; i < buttonParents.Length; i++)
        {
            startPositions[i] = buttonParents[i].anchoredPosition;

            if (i == buttonIndex)
            {
                targetPositions[i] = new Vector3(initialPositions[i].x, targetYPositions[buttonIndex], initialPositions[i].z);
            }
            else if (i < buttonIndex)
            {
                // If the button is above the current button, it should move to its up position
                targetPositions[i] = new Vector3(initialPositions[i].x, targetYPositions[i], initialPositions[i].z);
            }
            else
            {
                targetPositions[i] = initialPositions[i];
            }


        }

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            for (int i = 0; i < buttonParents.Length; i++)
            {
                buttonParents[i].anchoredPosition = Vector3.Lerp(startPositions[i], targetPositions[i], t / moveDuration);
            }
            yield return null;
        }

        for (int i = 0; i < buttonParents.Length; i++)
        {
            buttonParents[i].anchoredPosition = targetPositions[i];
        }
        isMoving = false;

        // Rotate the arrows based on whether the button is moving up or down
        for (int i = 0; i < buttonArrows.Length; i++)
        {
            if (i == buttonIndex)
            {
                buttonArrows[i].transform.localEulerAngles = new Vector3(180, 0, 0);
            }
            else
            {
                buttonArrows[i].transform.localEulerAngles = new Vector3(0, 0, 0);
            }
        }
    }
    public void OnButtonClick(int buttonIndex)
    {
        if (isMoving)
        {
            return;
        }

        if (activeButtonIndex == buttonIndex)
        {
            // If the same button is clicked again, reset all button positions.
            buttonIndex = -1;
        }

        StartCoroutine(MoveButtons(buttonIndex));
        activeButtonIndex = buttonIndex;

        // Reset all arrows to their initial rotation
        for (int i = 0; i < buttonArrows.Length; i++)
        {
            buttonArrows[i].transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        // Rotate the arrow of the active button if there's an active button
        if (activeButtonIndex != -1)
        {
            buttonArrows[activeButtonIndex].transform.localEulerAngles = new Vector3(180, 0, 0);
        }
    }
}
