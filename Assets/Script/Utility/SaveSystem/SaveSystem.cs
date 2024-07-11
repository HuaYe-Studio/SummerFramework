using UnityEngine;
using System.IO;

namespace Utility.SaveSystem
{
    public class SaveSystem
    {
        public static void Save<T>(T data, string path)
        {
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
        }

        public static void SaveToPersistent<T>(T data, string path)
        {
            Save(data, Application.persistentDataPath + path);
        }

        public static void SaveToStreamingAssets<T>(T data, string path)
        {
            Save(data, Application.streamingAssetsPath + path);
        }

        public static T Load<T>(string path)
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
    }
}