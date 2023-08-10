using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MirrorScript : MonoBehaviour
{
    public StartPartController controller;
    public bool slideMirrored = false;
    public RawImage imageToChangeColor;
    public Texture normalTexture;
    public Texture mirroredTexture;
    private HierarchyBounds bounds;
    void Start()
    {
        bounds = GameObject.Find("CommandManager").GetComponent<HierarchyBounds>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void MirrorSlide()
    {
        if (!slideMirrored)
        {
            Transform startPartChild = controller.startPart.transform.Find("SnapPoint");
            startPartChild.transform.localScale = new Vector3(startPartChild.transform.localScale.x, startPartChild.transform.localScale.y, -1);
            imageToChangeColor.texture = mirroredTexture;
            slideMirrored = true;
            bounds.CalculateBounds();
        }
        else if (slideMirrored)
        {
            Transform startPartChild = controller.startPart.transform.Find("SnapPoint");
            startPartChild.transform.localScale = new Vector3(startPartChild.transform.localScale.x, startPartChild.transform.localScale.y, 1);
            imageToChangeColor.texture = normalTexture;
            slideMirrored = false;
            bounds.CalculateBounds();
        }
    }
}
