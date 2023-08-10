using System.Collections.Generic;
using UnityEngine;

public class JsonHelper
{
    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }

    public static List<T> FromJson<T>(string jsonString)
    {
        string jsonAsArray = "{\"items\":" + jsonString + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(jsonAsArray);
        return new List<T>(wrapper.items);
    }


    public static string ToJson<T>(List<T> list)
    {
        Wrapper<T> wrapper = new Wrapper<T> { items = list.ToArray() };
        return JsonUtility.ToJson(wrapper);
    }
}
