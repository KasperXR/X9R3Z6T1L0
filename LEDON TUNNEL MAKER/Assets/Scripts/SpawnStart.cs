using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpawnStart : MonoBehaviour
{
    public TMP_InputField meterValue;
    public GameObject Prefab;
    public Button Button1;
    public Button Button2;
    public Button Button3;
    public Button[] ColorButtons;
    public Color DefaultButtonColor;
    public Color ActiveButtonColor;
    public GameObject ButtonBorder;
    public StartPartController startPartController;
    private OrbitControls orbitControls;
    public GameObject existingPrefab;
    private string Model2Name = "model-startPart-curved"; // Replace with the actual name of Model1 in the prefab
    private string Model1Name = "model-startPart-standard"; // Replace with the actual name of Model2 in the prefab
    public Material[] BodyMaterials;
    public Material[] NutsMaterials;
    public int selectedMaterialIndex = 0;
    private Indicator indicator;
    private void Start()
    {
        startPartController = GameObject.Find("CommandManager").GetComponent<StartPartController>();
        indicator = GameObject.Find("Indicator").GetComponent<Indicator>();
        orbitControls = FindObjectOfType<OrbitControls>();
        if (orbitControls == null)
        {
            Debug.LogError("OrbitControls component not found in the scene.");
        }

        // Initialize default button and model
        SetActiveModel(1);
    }

    public void SpawnStartObject()
    {
        if (meterValue != null)
        {
            float y = float.Parse(meterValue.text) / 100f;

            // Check if the existingPrefab object is already initialized
            if (existingPrefab != null)
            {
                // Update the position of the existing prefab
                existingPrefab.transform.position = new Vector3(0, y + 0.3805935f, 0);
                startPartController.SetStartPart(existingPrefab);
            }
            else
            {
                // Find the prefab in the scene by its name
                existingPrefab = GameObject.Find(Prefab.name.Replace("(Clone)", ""));

                if (existingPrefab != null)
                {
                    // Update the position of the existing prefab
                    existingPrefab.transform.position = new Vector3(0, y + 0.3805935f, 0);
                    startPartController.SetStartPart(existingPrefab);
                    startPartController.SaveData();
                    indicator.MoveToLastSnapPoint();
                }
                else
                {
                    // Instantiate a new prefab
                    existingPrefab = Instantiate(Prefab, new Vector3(0, y + 0.3805935f, 0), Quaternion.identity);
                    // Assign a color immediately after spawning
                    ChangeMaterial(selectedMaterialIndex); // change this to your selected color index
                    existingPrefab.name = Prefab.name.Replace("(Clone)", ""); // remove "Clone" from the prefab's name
                    startPartController.SetStartPart(existingPrefab);
                    startPartController.SaveData();
                    indicator.MoveToLastSnapPoint();
                    orbitControls.LerpToTargetHeight(y);
                }
            }

            // Update the target position after initializing the existingPrefab
            if (orbitControls != null)
            {
                orbitControls.LerpToTargetHeight(y);
                

            }
            if (existingPrefab == null)
            {
                orbitControls.LerpToTargetHeight(1f);
            }
        }
    }

    public void SetActiveModel(int modelIndex)
    {
        GameObject model1, model2, nuts;

        if (existingPrefab == null)
        {
            model1 = Prefab.transform.Find(Model1Name).gameObject;
            model2 = Prefab.transform.Find(Model2Name).gameObject;
            nuts = Prefab.transform.Find("Origin_nuts").gameObject;
        }
        else
        {
            model1 = existingPrefab.transform.Find(Model1Name).gameObject;
            model2 = existingPrefab.transform.Find(Model2Name).gameObject;
            nuts = existingPrefab.transform.Find("Origin_nuts").gameObject;
        }

        model1.SetActive(modelIndex == 1);
        model2.SetActive(modelIndex == 2);
        nuts.SetActive(modelIndex != 0); 

        Button1.GetComponent<RawImage>().color = modelIndex == 1 ? ActiveButtonColor : DefaultButtonColor;
        Button2.GetComponent<RawImage>().color = modelIndex == 2 ? ActiveButtonColor : DefaultButtonColor;
        Button3.GetComponent<RawImage>().color = modelIndex == 0 ? ActiveButtonColor : DefaultButtonColor;
    }
    public void ChangeColorRed()
    {
        selectedMaterialIndex = 0;
        ChangeMaterial(0);
        ButtonBorder.transform.localPosition = new Vector3(-0.541000009f, 0.0500000007f, -0.00179999997f);
    }

    public void ChangeColorBlue()
    {
        selectedMaterialIndex = 1;
        ChangeMaterial(1);
        ButtonBorder.transform.localPosition = new Vector3(-0.319999993f, 0.0500000007f, -0.00179999997f);
    }

    public void ChangeColorYellow()
    {
        selectedMaterialIndex = 2;
        ChangeMaterial(2);
        ButtonBorder.transform.localPosition = new Vector3(-0.0930000022f, 0.0500000007f, -0.00179999997f);
    }

    public void ChangeColorGreen()
    {
        selectedMaterialIndex = 3;
        ChangeMaterial(3);
        ButtonBorder.transform.localPosition = new Vector3(0.140000001f, 0.0500000007f, -0.00179999997f);
    }

    public void ChangeColorGrey()
    {
        selectedMaterialIndex = 4;
        ChangeMaterial(4);
        ButtonBorder.transform.localPosition = new Vector3(0.352999985f, 0.0500000007f, -0.00179999997f);
    }

    private void ChangeMaterial(int materialIndex)
    {
        if (materialIndex < 0 || materialIndex >= BodyMaterials.Length || materialIndex >= NutsMaterials.Length)
        {
            Debug.LogError("Invalid material index");
            return;
        }

        Material bodyMaterial = BodyMaterials[materialIndex];
        Material nutsMaterial = NutsMaterials[materialIndex];

        GameObject model1, model2;
        Renderer nutsRenderer;

        if (existingPrefab == null)
        {
            model1 = Prefab.transform.Find(Model1Name).gameObject;
            model2 = Prefab.transform.Find(Model2Name).gameObject;
            nutsRenderer = Prefab.transform.Find("Origin_nuts").GetComponent<Renderer>();
        }
        else
        {
            model1 = existingPrefab.transform.Find(Model1Name).gameObject;
            model2 = existingPrefab.transform.Find(Model2Name).gameObject;
            nutsRenderer = existingPrefab.transform.Find("Origin_nuts").GetComponent<Renderer>();
        }

        Renderer model1Renderer = model1.GetComponent<Renderer>();
        Renderer model2Renderer = model2.GetComponent<Renderer>();

        if (model1Renderer == null || model2Renderer == null)
        {
            Debug.LogError("Renderer component not found on model");
            return;
        }

        Material[] model1Materials = model1Renderer.sharedMaterials;
        Material[] model2Materials = model2Renderer.sharedMaterials;

        for (int i = 0; i < model1Materials.Length; i++)
        {
            model1Materials[i] = bodyMaterial;
        }

        for (int i = 0; i < model2Materials.Length; i++)
        {
            model2Materials[i] = bodyMaterial;
        }

        model1Renderer.sharedMaterials = model1Materials;
        model2Renderer.sharedMaterials = model2Materials;
        nutsRenderer.material = nutsMaterial;

        // If existingPrefab is null, update the prefab's materials as well
        if (existingPrefab == null)
        {
            Renderer prefabNutsRenderer = Prefab.transform.Find("Origin_nuts").GetComponent<Renderer>();
            Renderer prefabModel1Renderer = Prefab.transform.Find(Model1Name).gameObject.GetComponent<Renderer>();
            Renderer prefabModel2Renderer = Prefab.transform.Find(Model2Name).gameObject.GetComponent<Renderer>();

            Material[] prefabModel1Materials = prefabModel1Renderer.sharedMaterials;
            Material[] prefabModel2Materials = prefabModel2Renderer.sharedMaterials;

            for (int i = 0; i < prefabModel1Materials.Length; i++)
            {
                prefabModel1Materials[i] = bodyMaterial;
            }

            for (int i = 0; i < prefabModel2Materials.Length; i++)
            {
                prefabModel2Materials[i] = bodyMaterial;
            }

            prefabModel1Renderer.sharedMaterials = prefabModel1Materials;
            prefabModel2Renderer.sharedMaterials = prefabModel2Materials;
            prefabNutsRenderer.material = nutsMaterial;
        }
    }
    private void FixedUpdate()
    {
        if(existingPrefab == null)
        {
            existingPrefab = GameObject.Find("StartPart");
        }
    }
}