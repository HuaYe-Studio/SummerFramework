#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleFramework.BuffSystem.BuffBase;
using BattleFramework.BuffSystem.BuffTag;
using BattleFramework.BuffSystem.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Utility.LogSystem;
using System.IO;

namespace BattleFramework.BuffSystem.Editor
{
    public class BuffEditor : OdinEditorWindow
    {
        #region Fields

        private int _selectedBuffIndex = -1;
        private BuffCollection _buffCollectionSo;
        private SerializedObject _serializedObject;
        private SerializedObject _currentBuffSo;
        private SerializedProperty _currentBuffProp;

        //保存
        private static readonly string DefaultSoName = "BuffCollection.asset";
        private static readonly string DefaultTagSoName = "BuffTagData.asset";
        private int _maxLengthBuffer;
        private GameObject _buffManager;
        private BitBuffTagData _bitBuffTagData;

        private static string BuffsPath => ScriptObjectPath + "/Buffs";

        //程序集
        private const string AssemblyName = "Assembly-CSharp";
        private Assembly _assembly;

        //子类选项
        private int _selectedSubclassIndex = 0;
        private int _lastSelectSubclassIndex = 0;
        private bool _changedSubClassSelection = false;
        private const string PlaceholderBuffName = "PlaceholderBuff";
        private const string NoneType = "[NONE]";

        private List<Type> _subclassTypes;
        string[] _subclassNames;

        #endregion


        [MenuItem("BuffSystem/BuffEditor")]
        private static void OpenWindow()
        {
            var window = GetWindow<BuffEditor>();
            window.titleContent = new GUIContent("Buff编辑器");
            window.Init();
        }

        public static string ScriptObjectPath
        {
            get
            {
                var buffManagerPrefab = AssetDatabase.FindAssets("BuffManager t:Prefab");
                switch (buffManagerPrefab.Length)
                {
                    case <= 0:
                        throw new Exception("BuffManager prefab not found");
                    case > 1:
                        LogSystem.Instance.Log("More than one BuffManager prefab found", LogLevelEnum.Error);
                        break;
                }

                var path = AssetDatabase.GUIDToAssetPath(buffManagerPrefab[0])
                    .Replace("BuffManager.prefab", "Data/BuffData");
                return path;
            }
        }

        private void Init()
        {
            //Debug.Log(ScriptObjectPath);
            UpdateSubClass();
            UpdateBuffList();
            CreatBuffTagAsset();
            UpdateBuffManager();
        }

        #region 更新方法

        private void UpdateBuffManager()
        {
            var manager = AssetDatabase.FindAssets("BuffManager t:Prefab");
            if (manager.Length <= 0)
                LogSystem.Instance.Log("BuffManager prefab not found", LogLevelEnum.Error);
            if (manager.Length > 1)
                LogSystem.Instance.Log("More than one BuffManager prefab found", LogLevelEnum.Error);
            string assetPath = AssetDatabase.GUIDToAssetPath(manager[0]);
            _buffManager = PrefabUtility.LoadPrefabContents(assetPath);
            _buffManager.GetComponent<BuffManager>().SetData(_buffCollectionSo);
            _buffManager.GetComponent<BitBuffTagManager>().SetData(_bitBuffTagData);
            PrefabUtility.SaveAsPrefabAsset(_buffManager, assetPath);
            PrefabUtility.UnloadPrefabContents(_buffManager);
        }

        private void UpdateSubClass()
        {
            _selectedSubclassIndex = 0;
            _lastSelectSubclassIndex = 0;
            _changedSubClassSelection = false;
            _subclassTypes = _assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BuffInfo)) && !t.IsAbstract)
                .ToList();
            _subclassNames = _subclassTypes.Select(c => c.Name).ToArray();
            var s = _subclassNames[0];
            int i = Array.IndexOf(_subclassNames, PlaceholderBuffName);
            _subclassNames[i] = s;
            _subclassNames[0] = NoneType;
        }

        private void UpdateBuffList()
        {
            string[] assetPaths = AssetDatabase.FindAssets
                ("t:BuffCollection", new string[] { ScriptObjectPath });
            // Debug.Log(assetPaths.Length);
            // Debug.Log(assetPaths[0]);
            if (assetPaths.Length <= 0)
            {
                if (!Directory.Exists(ScriptObjectPath))
                    Directory.CreateDirectory(ScriptObjectPath);
                _buffCollectionSo = CreateInstance<BuffCollection>();
                var path = Path.Combine(ScriptObjectPath, DefaultSoName);
                AssetDatabase.CreateAsset(_buffCollectionSo, path);
                UpdateBuffManager();
            }
            else
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
                _buffCollectionSo = AssetDatabase.LoadAssetAtPath<BuffCollection>(assetPath);
            }

            _buffCollectionSo.ReSize();
            if (!Directory.Exists(BuffsPath))
                Directory.CreateDirectory(BuffsPath);
            _maxLengthBuffer = _buffCollectionSo.buffList.Count;
            _serializedObject = new SerializedObject(_buffCollectionSo);
        }

        #endregion

        #region 保存Asset方法

        private void CreatBuffAsset(int index)
        {
            var fileName = "Buff" + $"{index:000}" + ".asset";
            var directory = Path.Combine(BuffsPath, fileName);
            if (File.Exists(directory)) File.Delete(directory);
            var buff = _buffCollectionSo.buffList[index];
            AssetDatabase.CreateAsset(buff, directory);
        }

        private void CreatBuffTagAsset()
        {
            var directory = Path.Combine(ScriptObjectPath, DefaultTagSoName);
            if (File.Exists(directory))
            {
                _bitBuffTagData = AssetDatabase.LoadAssetAtPath<BitBuffTagData>(directory);
                return;
            }

            BitBuffTagData data = CreateInstance<BitBuffTagData>();
            data.Init();
            AssetDatabase.CreateAsset(data, directory);
            _bitBuffTagData = data;
            UpdateBuffManager();
        }

        #endregion
    }
}
#endif