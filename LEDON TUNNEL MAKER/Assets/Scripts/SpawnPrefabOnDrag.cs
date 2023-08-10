using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpawnPrefabOnDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public GameObject prefab;

    private bool isDragging = false;
    private bool isWithinUI = true;
    private GameObject spawnedObject;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isDragging = true;
            isWithinUI = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            if (GetComponent<RectTransform>().rect.Contains(localPoint))
            {
                isWithinUI = true;
            }
            else if (isWithinUI)
            {
                isWithinUI = false;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                spawnedObject = Instantiate(prefab, worldPosition, Quaternion.identity);
            }

            if (!isWithinUI)
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = -Camera.main.transform.position.z;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(prefab, worldPosition, Quaternion.identity);
                }
                else
                {
                    spawnedObject.transform.position = worldPosition;
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isDragging = false;
            if (spawnedObject != null && spawnedObject.transform.position.magnitude > 10f)
            {
                Destroy(spawnedObject);
            }
        }
    }
}