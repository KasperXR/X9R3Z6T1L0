using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ES3Types;
using ES3Internal;

public class UndoRedo : MonoBehaviour
{
    private List<GameObject> prefabs;
    private List<List<GameObject>> historyPrefabs;
    private int currentIndex;

    // Start is called before the first frame update
    void Start()
    {
        prefabs = new List<GameObject>();
        historyPrefabs = new List<List<GameObject>>();
        currentIndex = -1;

        

        StartCoroutine(FetchPrefabsRoutine());
    }

    IEnumerator FetchPrefabsRoutine()
    {
        while (true)
        {
            FetchPrefabs();
            yield return new WaitForSeconds(5f);
        }
    }

    void FetchPrefabs()
    {
        prefabs.Clear();
        GameObject startPart = GameObject.Find("StartPart");

        if (startPart != null)
        {
            if (startPart.tag == "Straight" || startPart.tag == "Curved")
            {
                prefabs.Add(startPart);
            }

            RecurseChildObjects(startPart.transform);
        }

        Debug.Log("Found " + prefabs.Count + " prefabs:");
        foreach (var prefab in prefabs)
        {
            Debug.Log(prefab.name);
        }

        historyPrefabs.Add(new List<GameObject>(prefabs));
        currentIndex++;

        ES3.Save("prefabs", historyPrefabs);
    }

    void RecurseChildObjects(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.tag == "Straight" || child.tag == "Curved")
            {
                prefabs.Add(child.gameObject);
            }

            if (child.childCount > 0)
            {
                RecurseChildObjects(child);
            }
        }
    }

    public void OnButtonClick()
    {
        if (ES3.KeyExists("prefabs"))
        {
            GameObject firstPrefab = GameObject.Find("StartPart");
            Destroy(firstPrefab);
            historyPrefabs = ES3.Load<List<List<GameObject>>>("prefabs");

            if (currentIndex >= 0)
            {
                prefabs = historyPrefabs[currentIndex];
                currentIndex--;
            }
            else
            {
                Debug.Log("No more history to load.");
            }
        }
        else
        {
            Debug.Log("No prefabs history to load.");
        }
    }
}
