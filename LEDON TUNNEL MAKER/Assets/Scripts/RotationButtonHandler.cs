using UnityEngine;
using UnityEngine.EventSystems;

public class RotationButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ObjectRotator objectRotator;
    public bool rotateLeft;

    public void SetRotateDirection(bool isLeft)
    {
        rotateLeft = isLeft;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (rotateLeft)
        {
            objectRotator.SetRotateLeft(true);
        }
        else
        {
            objectRotator.SetRotateRight(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (rotateLeft)
        {
            objectRotator.SetRotateLeft(false);
        }
        else
        {
            objectRotator.SetRotateRight(false);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (rotateLeft)
        {
            objectRotator.SingleRotateLeft();
        }
        else
        {
            objectRotator.SingleRotateRight();
        }
    }
}