using UnityEngine;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Utility.LogSystem;
using Utility.SingletonPatternSystem;

namespace Utility.SaveSystem
{
    public class SaveSystem : LazySingleton<SaveSystem>
    {
        #region 同步版本

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

        public static T Load<T>(string name, string path, CancellationToken? token = null)
        {
            if (!File.Exists(path + "/" + name))
            {
                LogSystem.LogSystem.Instance.Log($"{name}不存在于路径{path}中", LogLevelEnum.Debug);
            }

            var json = File.ReadAllText(path);
            return string.IsNullOrEmpty(json) ? default : JsonUtility.FromJson<T>(json);
        }

        #endregion

        #region 异步版本

        public static async UniTask SaveAsync<T>(T data, string name, string path, CancellationToken? token = null)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                LogSystem.LogSystem.Instance.Log($"创建文件夹{path}", LogLevelEnum.Debug);
            }

            string json = JsonUtility.ToJson(data);
            await File.WriteAllTextAsync(path + "/" + name, json, token ?? CancellationToken.None).ConfigureAwait(true);
        }

        public static async UniTask SaveToPersistentAsync<T>(T data, string name, string path)
        {
            await SaveAsync(data, name, Application.persistentDataPath + path);
        }

        public static async void SaveToStreamingAssetsAsync<T>(T data, string name, string path)
        {
            await SaveAsync(data, name, Application.streamingAssetsPath + path);
        }

        public static async UniTask<T> LoadAsync<T>(string name, string path, CancellationToken? token = null)
        {
            if (!File.Exists(path + "/" + name))
            {
                LogSystem.LogSystem.Instance.Log($"{name}不存在于路径{path}中", LogLevelEnum.Debug);
            }

            var json = await File.ReadAllTextAsync(path, token ?? CancellationToken.None).ConfigureAwait(true);
            return string.IsNullOrEmpty(json) ? default : JsonUtility.FromJson<T>(json);
        }

        #endregion
    }
}