using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{
    public List<GameObject> prefabs;
    public List<Button> buttons;
    public List<RawImage> imagesOfObjects;
    public GameObject buttonBorder;
    public List<Material> mainMaterials;
    public List<Material> nutsMaterials;
    public List<Material> windowMaterials;

    private int selectedButtonIndex = -1;

    private void Start()
    {
        ChangeColor(2);

        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => ChangeColor(index));
        }
    }

    public void ChangeColor(int index)
    {
        if (index < 0 || index >= mainMaterials.Count)
        {
            Debug.LogError("Invalid color index");
            return;
        }

        Material material = mainMaterials[index];
        foreach (GameObject prefab in prefabs)
        {
            ChangePrefabMaterial(prefab.transform, material, index);
        }
        foreach (RawImage rawImage in imagesOfObjects)
        {
            rawImage.color = material.color;
        }

        HighlightSelectedButton(index);
    }

    private void ChangePrefabMaterial(Transform transform, Material material, int index)
    {
        Renderer renderer = transform.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material newMaterial = new Material(material);

            if (transform.name == "Origin_nuts" || transform.name == "SnapPoint_nuts")
            {
                newMaterial = nutsMaterials[index];
            }
            else
            {
                newMaterial = mainMaterials[index];
            }

            renderer.sharedMaterial = newMaterial;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            ChangePrefabMaterial(transform.GetChild(i), material, index);
        }
    }

    private void HighlightSelectedButton(int index)
    {
        if (selectedButtonIndex != -1)
        {
            ColorBlock cb = buttons[selectedButtonIndex].colors;
            cb.normalColor = Color.white;
            buttons[selectedButtonIndex].colors = cb;
        }

        ColorBlock colorBlock = buttons[index].colors;
        colorBlock.normalColor = mainMaterials[index].color;
        buttons[index].colors = colorBlock;

        RectTransform buttonRectTransform = buttons[index].GetComponent<RectTransform>();
        buttonBorder.transform.position = buttonRectTransform.position;

        selectedButtonIndex = index;
    }
}
