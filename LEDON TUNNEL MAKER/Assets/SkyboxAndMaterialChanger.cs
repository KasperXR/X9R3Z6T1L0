using UnityEngine;

public class SkyboxAndMaterialChanger : MonoBehaviour
{
    [SerializeField] private Material[] skyboxMaterials;
    [SerializeField] private GameObject[] planes;
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        // At the start, only the first plane (index 0) is enabled.
        planes[0].SetActive(true);
        for (int i = 1; i < planes.Length; i++)
        {
            planes[i].SetActive(false);
        }
    }

    public void ChangeSkybox(int index)
    {
        if (index >= 0 && index < skyboxMaterials.Length)
        {
            Skybox skyboxComponent = mainCamera.GetComponent<Skybox>();
            skyboxComponent.material = skyboxMaterials[index];
            RenderSettings.skybox = skyboxMaterials[index];
            DynamicGI.UpdateEnvironment();
        }
    }

    public void ChangePlane(int index)
    {
        if (index >= 0 && index < planes.Length)
        {
            // Disable all planes
            for (int i = 0; i < planes.Length; i++)
            {
                planes[i].SetActive(false);
            }

            // Enable the selected plane
            planes[index].SetActive(true);
        }
    }
}
