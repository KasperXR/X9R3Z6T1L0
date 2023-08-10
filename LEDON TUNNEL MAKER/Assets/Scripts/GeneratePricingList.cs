using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using UnityEngine.Networking;
using Unity.Collections;

public class GeneratePricingList : MonoBehaviour
{
    public GameObject objectItemPrefab;
    public GameObject objectItemPrefab2;
    public ScrollRect objectListScrollView;
    public ScrollRect objectListScrollView2;
    public TextMeshProUGUI totalPriceText;
    public TextMeshProUGUI totalPriceText2;
    public float totalPrice;
    public float totalPrice2;
    private UIManager manager;
    public string url;
    public string currency;
    private Dictionary<string, float> objectPrices = new Dictionary<string, float>();
    private Dictionary<string, int> objectCounts = new Dictionary<string, int>();
    private Dictionary<string, string> objectCodes = new Dictionary<string, string>()
{
    { "model-straight-35", "6602" },
    { "model-straight-59", "6604" },
    { "model-straight-59-window", "6605" },
    { "model-straight-106", "6606" },
    { "model-straight-106-window", "6607" },
    { "model-curved-32", "6610" },
    { "model-curved-38", "6612" },
    { "model-curved-45", "6614" },
    { "model-startPart-standard", "2000" },
    { "model-startPart-curved", "3000" },
    { "model-ending", "6616" },
    { "model-ending-middle", "6646" },
    { "model-ending-end", "6647" },
    { "SupportBeam-single-66", "6600,011" },
    { "SupportBeam-single-90", "6600,012" },
    { "SupportBeam-single-127", "6600,013" },
    { "SupportBeam-single-163", "6600,014" },
    { "SupportBeam-single-220", "6600,015" },
    { "SupportBeam-single-280", "6600,016" },
    { "SupportBeam-single-320", "6600,017" },
    { "SupportBeam-single-420", "6600,018" },
    { "SupportBeam-double-66", "6600,011" },
    { "SupportBeam-double-90", "6600,012" },
    { "SupportBeam-double-127", "6600,013" },
    { "SupportBeam-double-163", "6600,014" },
    { "SupportBeam-double-220", "6600,015" },
    { "SupportBeam-double-280", "6600,016" },
    { "SupportBeam-double-320", "6600,017" },
    { "SupportBeam-double-420", "6600,018" },
    { "SupportBeam-middle-66", "6600,011" },
    { "SupportBeam-middle-90", "6600,012" },
    { "SupportBeam-middle-127", "6600,013" },
    { "SupportBeam-middle-163", "6600,014" },
    { "SupportBeam-middle-220", "6600,015" },
    { "SupportBeam-middle-280", "6600,016" },
    { "SupportBeam-middle-320", "6600,017" },
    { "SupportBeam-middle-420", "6600,018" },
    { "SupportBeam-Unsupported", "6600-Unsupported" },
    
    

    // Add more objects here
};

    public GameObject startPart;
    public SpawnStart spawnStartScripts;

    private void Start()
    {
       
        
        manager = GameObject.Find("CommandManager").GetComponent<UIManager>();
    }

    public IEnumerator FetchAndSetupPrices()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://xplorxr.dk/LEDON/Scripts/getPrices.php");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Received: " + www.downloadHandler.text);
            List<PriceItem> prices = JsonHelper.FromJson<PriceItem>(www.downloadHandler.text);

            foreach (PriceItem item in prices)
            {
                float price = (manager.accountLocation == "Export") ? item.price_EUR : item.price_DKK; // Pick the correct price based on location


                if (objectPrices.ContainsKey(item.product_name))
                {
                    objectPrices[item.product_name] = price;
                }
                else
                {
                    objectPrices.Add(item.product_name, price);
                }
            }
            if (manager.accountLocation == "Export")
            {
                objectPrices.Add("6600,18", 152);
                objectPrices.Add("6600,11", 80); // Add the price for "6600,11" in EUR
            }
            else
            {
                objectPrices.Add("6600,18", 1136);
                objectPrices.Add("6600,11", 594); // Add the price for "6600,11" in DKK
            }
        }
    }

    private void FixedUpdate()
    {
        if (startPart == null)
        {
            startPart = GameObject.Find("StartPart");
        }
    }

    public IEnumerator UpdateListCoroutine()
    {
        
            while (true)
            {  
                    try
                    {
                        GenerateList();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in GenerateList: {e}");
                    }
                    yield return new WaitForSeconds(1f);
                
                
            }
        
    }

    public void GenerateList()
    {
        foreach (Transform child in objectListScrollView.content)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in objectListScrollView2.content)
        {
            Destroy(child.gameObject);
        }

        totalPrice = 0;
        totalPrice2 = 0;
        objectCounts.Clear();
        List<GameObject> children = new List<GameObject>();
        FindChildrenWithTag(startPart, "Standard", ref children);
        FindChildrenWithTag(startPart, "Window", ref children);
        FindChildrenWithTag(startPart, "Beam", ref children);

        int doubleCount = 0; // Initialize counter for "double" type objects
        int singleBeamCount = 0; // Initialize counter for "SupportBeam-single" type objects

        foreach (GameObject child in children)
        {
            string type = child.name.Split('(')[0].Trim();

            if (objectPrices.ContainsKey(type))
            {
                if (objectCounts.ContainsKey(type))
                {
                    objectCounts[type]++;
                }
                else
                {
                    objectCounts[type] = 1;
                }

                if (type.Contains("double"))
                {
                    doubleCount += 1;
                }

                if (type.Contains("SupportBeam-single"))
                {
                    singleBeamCount += 1;
                }

                totalPrice += objectPrices[type];
            }
            else
            {
                Debug.Log($"Unrecognized object type: {type}");
            }
        }

        currency = (manager.accountLocation == "Export") ? "EUR" : "DKK";

        foreach (KeyValuePair<string, int> entry in objectCounts)
        {
            int multiplier = 1;
            if (entry.Key.Contains("double"))
            {
                multiplier = 2;
            }

            GameObject item = Instantiate(objectItemPrefab, objectListScrollView.content);
            TextMeshProUGUI textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"{objectCodes[entry.Key]} x{entry.Value * multiplier}";
            }
        }

        // Create "6600,18" entry based on `doubleCount`
        if (doubleCount > 0)
        {
            GameObject extraItem = Instantiate(objectItemPrefab, objectListScrollView.content);
            TextMeshProUGUI extraTextComponent = extraItem.GetComponentInChildren<TextMeshProUGUI>();
            if (extraTextComponent != null)
            {
                extraTextComponent.text = $"6600,18 x{doubleCount}";
                // If you have a price for "6600,18", don't forget to add it to totalPrice here.
            }
        }

        // Create "6600,11" entry based on `singleBeamCount`
        if (singleBeamCount > 0)
        {
            GameObject extraItem = Instantiate(objectItemPrefab, objectListScrollView.content);
            TextMeshProUGUI extraTextComponent = extraItem.GetComponentInChildren<TextMeshProUGUI>();
            if (extraTextComponent != null)
            {
                extraTextComponent.text = $"6600,11 x{singleBeamCount}";
                totalPrice += objectPrices["6600,11"] * singleBeamCount; // Add the total price for "6600,11"
            }
        }

        totalPriceText.text = $"EST. Price: {totalPrice} {currency}";

        foreach (KeyValuePair<string, int> entry in objectCounts)
        {
            GameObject item2 = Instantiate(objectItemPrefab2, objectListScrollView2.content);
            TextMeshProUGUI textComponent2 = item2.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent2 != null)
            {
                float objectPrice = objectPrices[entry.Key];
                int multiplier = 1;
                if (entry.Key.Contains("double"))
                {
                    multiplier = 2;
                }
                textComponent2.text = $"{objectCodes[entry.Key]} x{entry.Value * multiplier} - {objectPrice * entry.Value * multiplier} {currency}";
                totalPrice2 += objectPrice * entry.Value * multiplier;
            }
        }

        // Create "6600,18" entry based on `doubleCount`
        if (doubleCount > 0)
        {
            float extraPrice = objectPrices["6600,18"]; // Assuming you have a price for "6600,18" in your `objectPrices`
            GameObject extraItem = Instantiate(objectItemPrefab2, objectListScrollView2.content);
            TextMeshProUGUI extraTextComponent = extraItem.GetComponentInChildren<TextMeshProUGUI>();
            if (extraTextComponent != null)
            {
                extraTextComponent.text = $"6600,18 x{doubleCount} - {extraPrice * doubleCount} {currency}";
                totalPrice2 += extraPrice * doubleCount;
            }
        }

        // Create "6600,11" entry based on `singleBeamCount`
        if (singleBeamCount > 0)
        {
            float extraPrice = objectPrices["6600,11"]; // Assuming you have a price for "6600,11" in your `objectPrices`
            GameObject extraItem = Instantiate(objectItemPrefab2, objectListScrollView2.content);
            TextMeshProUGUI extraTextComponent = extraItem.GetComponentInChildren<TextMeshProUGUI>();
            if (extraTextComponent != null)
            {
                extraTextComponent.text = $"6600,11 x{singleBeamCount} - {extraPrice * singleBeamCount} {currency}";
                totalPrice2 += extraPrice * singleBeamCount;
            }
        }

        totalPriceText2.text = $"EST. Price: {totalPrice2} {currency}";

        DisplayDebugLogs();
    }


    private void FindChildrenWithTag(GameObject parent, string tag, ref List<GameObject> children)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.CompareTag(tag) && child.gameObject.activeInHierarchy)
            {
                children.Add(child.gameObject);
            }

            FindChildrenWithTag(child.gameObject, tag, ref children);
        }
    }

    private void DisplayDebugLogs()
    {
        string debugLog = "Object Counts:\n";
        foreach (KeyValuePair<string, int> entry in objectCounts)
        {
            debugLog += $"{entry.Key}: {entry.Value}\n";
        }

        Debug.Log(debugLog);
    }

    [Serializable]
    public class PriceItem
    {
        public string product_name;
        public float price_DKK;
        public float price_EUR;
    }

    public static class JsonHelper
    {
        public static List<T> FromJson<T>(string json)
        {
            string newJson = "{ \"items\": " + json + " }";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.items;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public List<T> items;
        }
    }
}
