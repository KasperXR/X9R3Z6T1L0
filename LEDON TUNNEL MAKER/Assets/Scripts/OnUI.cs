using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnUI : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
    [SerializeField]
    private UnityEvent onEnter;  // Event that is triggered when the pointer enters the UI element.

    [SerializeField]
    private UnityEvent onHover;  // Event that is triggered while the pointer is over the UI element.

    [SerializeField]
    private UnityEvent onExit;   // Event that is triggered when the pointer exits the UI element.

    // Properties to expose the UnityEvents to the Inspector.
    public UnityEvent OnEnter => onEnter;
    public UnityEvent OnHover => onHover;
    public UnityEvent OnExit => onExit;

    public bool UiEntered = false;

    // Update is called once per frame
    private void Update()
    {
        // Check if the pointer is over the UI element and onHover event is not null.

    }

    // Method called when the pointer enters the UI element.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null)
        {
            UiEntered = true;
            onEnter.Invoke();  // Trigger the onEnter event.
        }
    }

    // Method called when the pointer exits the UI element.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null)
        {
            UiEntered = false;
            onExit.Invoke();  // Trigger the onExit event.
        }
    }

    // Method to reset UiEntered flag
    public void ResetUiEntered()
    {
        UiEntered = false;
    }
}
