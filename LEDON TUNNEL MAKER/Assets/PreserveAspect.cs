using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(RawImage))]
public class PreserveAspect : MonoBehaviour
{
    public float originalWidth = 1;  // Set these to the original dimensions of your image
    public float originalHeight = 1;

    void Start()
    {
        AdjustSize();
    }

    void Update()
    {
        AdjustSize();
    }

    void AdjustSize()
    {
        var rectTransform = GetComponent<RectTransform>();
        var rawImage = GetComponent<RawImage>();

        // Calculate the aspect ratio of the texture
        float textureAspect = (float)rawImage.texture.width / rawImage.texture.height;

        // Calculate the aspect ratio of the original dimensions
        float originalAspect = originalWidth / originalHeight;

        if (textureAspect > originalAspect)
        {
            // The texture is wider than the original dimensions, so adjust the height to match the texture
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.x / textureAspect);
        }
        else
        {
            // The texture is taller than the original dimensions, so adjust the width to match the texture
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.y * textureAspect, rectTransform.sizeDelta.y);
        }
    }
}
