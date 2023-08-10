using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 10.0f;
    private Vector3 screenPoint;
    private Vector3 offset;
  
    

    private void Start()
    {
        
    }
    void Update()
    {
        
        if (Input.GetMouseButtonDown(1)) // Only move the camera if an object is not being dragged
        {
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        }

        if (Input.GetMouseButton(1)) // Only move the camera if an object is not being dragged
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            curPosition.y = transform.position.y;
            transform.position = Vector3.Lerp(transform.position, curPosition, speed * Time.deltaTime);
        }
    }
}
