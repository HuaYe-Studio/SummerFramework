using UnityEngine;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Utility.LogSystem;
using Utility.SingletonPatternSystem;

namespace Utility.SaveSystem
{
    public class SaveSystem : Singleton<SaveSystem>
    {
        public async static UniTask Save<T>(T data, string name, string path, CancellationToken? token = null)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                LogSystem.LogSystem.Instance.Log($"创建文件夹{path}", LogLevelEnum.Debug);
            }

            string json = JsonUtility.ToJson(data);
            await File.WriteAllTextAsync(path + "/" + name, json, token ?? CancellationToken.None).ConfigureAwait(true);
        }

        public static async UniTask SaveToPersistent<T>(T data, string name, string path)
        {
            await Save(data, name, Application.persistentDataPath + path);
        }

        public static async void SaveToStreamingAssets<T>(T data, string name, string path)
        {
            await Save(data, name, Application.streamingAssetsPath + path);
        }

        public static async UniTask<T> Load<T>(string name, string path, CancellationToken? token = null)
        {
            if (!File.Exists(path + "/" + name))
            {
                LogSystem.LogSystem.Instance.Log($"{name}不存在于路径{path}中", LogLevelEnum.Debug);
            }

            var json = await File.ReadAllTextAsync(path, token ?? CancellationToken.None).ConfigureAwait(true);
            return string.IsNullOrEmpty(json) ? default : JsonUtility.FromJson<T>(json);
        }
    }
}