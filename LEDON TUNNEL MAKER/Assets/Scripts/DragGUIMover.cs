using UnityEngine;
using UnityEngine.EventSystems;

public class DragGUIMover : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    private Vector2 offset;
    private RectTransform parentRectTransform;
    private Canvas canvas;
    public bool isDraggingGUI;
    public ObjectInfoCanvas objectInfoCanvas;

    private void Start()
    {
        // Make sure the current game object has a parent
        if (transform.parent == null)
        {
            Debug.LogError("DragGUIMover: No parent object found!");
            return;
        }

        // Get the RectTransform of the parent
        parentRectTransform = transform.parent.GetComponent<RectTransform>();

        // Add some error handling in case the parent doesn't have a RectTransform
        if (parentRectTransform == null)
        {
            Debug.LogError("DragGUIMover: No RectTransform found on parent object!");
            return;
        }

        canvas = GetComponentInParent<Canvas>();
        isDraggingGUI = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDraggingGUI = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out offset);
        if (objectInfoCanvas.positionLocked == false)
        {
            objectInfoCanvas.ToggleLock();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Vector3 newPosition = canvas.transform.TransformPoint(localPoint - offset);

            // Move the parent RectTransform
            parentRectTransform.position = newPosition;

            // Clamp the local x and y positions
            Vector3 localPos = parentRectTransform.localPosition;
            localPos.x = Mathf.Clamp(localPos.x, -500, 950);
            localPos.y = Mathf.Clamp(localPos.y, -500, 500);
            parentRectTransform.localPosition = localPos;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        isDraggingGUI = false;
        if (!objectInfoCanvas.positionLocked)
            objectInfoCanvas.ToggleLock(); // Lock position when GUI dragging is stopped if it's not locked already
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isDraggingGUI = false;
    }
}
