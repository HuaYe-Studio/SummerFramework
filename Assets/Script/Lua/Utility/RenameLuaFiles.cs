#if UNITY_EDITOR


using System.IO;
using UnityEditor;
using UnityEngine;
using Utility.LogSystem;

namespace Script.Lua.Utility
{
    public class RenameLuaFiles
    {
        [MenuItem("Tools/Rename Lua Files")]
        private static void RenameLuaFilesInProject()
        {
            var luaFiles = Directory.GetFiles(Application.dataPath, "*.lua", SearchOption.AllDirectories);

            foreach (var luaFile in luaFiles)
            {
                var newFileName = luaFile + ".txt";
                if (!File.Exists(newFileName))
                {
                    File.Move(luaFile, newFileName);
                    LogSystem.Instance.Log($"Renamed: {luaFile} to {newFileName}", LogLevelEnum.Debug);
                }
                else
                {
                    LogSystem.Instance.Log($"File already exists: {newFileName}", LogLevelEnum.Warning);
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif