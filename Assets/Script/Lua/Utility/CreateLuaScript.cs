using System.IO;
using UnityEditor;
using UnityEngine;
using Utility.LogSystem;

namespace Script.Lua.Utility
{
    public static class CreateLuaScript
    {
        [MenuItem("Assets/Create/Lua Script", false, 10)]
        private static void CreateLuaScriptAsset()
        {
            var path = GetSelectedPathOrFallback();
            var fileName = "NewLuaScript.lua";

            fileName = EditorUtility.SaveFilePanel("Save Lua Script", path, fileName, "lua");

            if (string.IsNullOrEmpty(fileName))
                return;

            if (File.Exists(fileName))
            {
                LogSystem.Instance.Log("Lua script already exists at this location.", LogLevelEnum.Debug);
                return;
            }

            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine("-- This is a new Lua script");
            }

            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(fileName);
        }

        private static string GetSelectedPathOrFallback()
        {
            var path = "Assets";

            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!File.Exists(path)) continue;
                path = Path.GetDirectoryName(path);
                break;
            }

            return path;
        }
    }
}