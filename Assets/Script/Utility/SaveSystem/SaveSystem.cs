using UnityEngine;
using System.IO;
using Utility.LogSystem;
using Utility.SingletonPatternSystem;

namespace Utility.SaveSystem
{
    public class SaveSystem : Singleton<SaveSystem>
    {
        public static void Save<T>(T data, string name, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                LogSystem.LogSystem.Instance.Log($"创建文件夹{path}", LogLevelEnum.Debug);
            }

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path + "/" + name, json);
        }

        public static void SaveToPersistent<T>(T data, string name, string path)
        {
            Save(data, name, Application.persistentDataPath + path);
        }

        public static void SaveToStreamingAssets<T>(T data, string name, string path)
        {
            Save(data, name, Application.streamingAssetsPath + path);
        }

        public static T Load<T>(string name, string path)
        {
            if (!File.Exists(path + "/" + name))
            {
                LogSystem.LogSystem.Instance.Log($"{name}不存在于路径{path}中", LogLevelEnum.Debug);
            }

            var json = File.ReadAllText(path);
            return string.IsNullOrEmpty(json) ? default : JsonUtility.FromJson<T>(json);
        }
    }
}