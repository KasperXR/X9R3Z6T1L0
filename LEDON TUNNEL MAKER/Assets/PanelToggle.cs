using System.Collections;
using UnityEngine;

public class PanelToggle : MonoBehaviour
{
    // Store the original position of the button
    private Vector3 originalPosition = new Vector3(-12.79f, -3.4f, 0);

    // Store whether the button is currently in its toggled position
    private bool isToggled = false;

    // The target position when the button is toggled
    public Vector3 toggledPosition = new Vector3(-11.5f, -3.4f, 0); // Modified to peek out from the side

    // The speed at which the button moves when toggling
    public float moveSpeed = 10f;

    private void Start()
    {
        // Store the original position of the button
    }

    public void ToggleButton()
    {
        // Calculate the target position based on whether the button is currently toggled
        Vector3 targetPosition = isToggled ? originalPosition : toggledPosition;

        // Set the toggled flag
        isToggled = !isToggled;

        // Move the button to the target position
        StartCoroutine(MoveButton(targetPosition));
    }

    private IEnumerator MoveButton(Vector3 targetPosition)
    {
        float t = 0f;

        // Calculate the distance between the button's current position and the target position
        Vector3 distanceVector = targetPosition - transform.localPosition;
        float distance = distanceVector.magnitude;

        // Check if the distance is small enough to consider the button already at the target position
        if (Mathf.Approximately(distance, 0f))
        {
            yield break;
        }

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed / distance;
            t = Mathf.Clamp01(t); // Clamp t between 0 and 1

            // Move the button smoothly using Mathf.SmoothStep()
            float smoothStepValue = Mathf.SmoothStep(0f, 1f, t);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smoothStepValue);

            // Check if the button is close enough to the target position to snap to it
            float threshold = 0.01f;
            if (Vector3.Distance(transform.localPosition, targetPosition) < threshold)
            {
                transform.localPosition = targetPosition;
                break;
            }

            yield return null;
        }

        // If the button is moving back to its original position, set isToggled to false
        if (targetPosition == originalPosition)
        {
            isToggled = false;
        }
    }
}