using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Utility.LogSystem.ReadFromCsv
{
    public class ReadFromCsvSystem : MonoBehaviour
    {
        [ShowInInspector] [Header("Csv文件路径")] [LabelText("文件列表")]
        public string[] filePath;


        [Button]
        public void ReadFromCsv(string[] csvPath)
        {
            for (var i = 0; i <= csvPath.Length; i++)
            {
                if (csvPath[i] == null)
                {
                    LogSystem.Instance.Log($"{filePath[i]} 不存在，请检查路径", LogLevelEnum.Debug);
                }
            }

            foreach (var path in csvPath)
            {
                var csv = File.ReadAllText(path);
                string[] dataRow = csv.Split('\n'); //按行分割
                foreach (var row in dataRow)
                {
                    //  CreatScriptableObject();
                    string[] rowArray = row.Split(','); //按列分割
                    if (rowArray[0] == "") //跳过第一行
                    {
                        continue;
                    }
                    else if (rowArray[0] == "")
                    {
                    }
                    else if (rowArray[1] == "")
                    {
                    }
                }
            }
        }

        private void CreatScriptableObject()
        {
            var scriptableObject = ScriptableObject.CreateInstance<ScriptableObject>();
            AssetDatabase.CreateAsset(scriptableObject, "Assets/ScriptableObject/ScriptableObject.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}