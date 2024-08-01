#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Utility.LogSystem;
using System.IO;
using BattleFramework.BuffSystem.BuffBase;
using BattleFramework.BuffSystem.BuffTag;
using BattleFramework.BuffSystem.Manager;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace BattleFramework.BuffSystem.Editor
{
    public class BuffEditor : OdinEditorWindow
    {
        #region Fields

        private BuffCollection _buffCollectionSo;
        private SerializedObject _serializedObject;
        private SerializedObject _currentBuffSo;
        private SerializedProperty _currentBuffProp;

        //保存
        private static readonly string DefaultSoName = "BuffCollection.asset";
        private static readonly string DefaultTagSoName = "BuffTagData.asset";

        private GameObject _buffManager;
       private BitBuffTagData _bitBuffTagData;

      

        private static string BuffsPath => ScriptObjectPath + "/Buffs";

        //程序集
        private const string AssemblyName = "Assembly-CSharp";
        private Assembly _assembly;

        //子类选项
        private int _selectedSubclassIndex;
        private int _lastSelectSubclassIndex;
        private bool _changedSubClassSelection;
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
        }

        protected override void OnEnable()
        {
            Init();
        }

        #region UI

        private const float WidthDivision = 0.25f;
        private Vector2 _leftScrollPos;
        private int _maxLengthBuffer;
        private int _selectedBuffIndex = -1;
        private bool _applyToAll;

        //buff id 前缀
        private int _buffIdPrefix;

        #endregion

        protected override void DrawEditors()
        {
            float leftWidth = position.width * WidthDivision;
            if (leftWidth >= 224.0f)
            {
                leftWidth = position.width * WidthDivision;
            }
            else
            {
                leftWidth = 224.0f;
            }

            Rect leftRect = new Rect(0, 0, leftWidth, position.height);
            GUILayout.BeginArea(leftRect, EditorStyles.helpBox);
            _leftScrollPos = GUILayout.BeginScrollView(_leftScrollPos);
            GUIStyle leftListButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft
            };
            //内容      
            for (int i = 0; i < _buffCollectionSo.Size; i++)
            {
                leftListButtonStyle.normal.textColor = i == _selectedBuffIndex ? Color.green : Color.white;
                if (GUILayout.Button(new GUIContent($"{i:000}" + ":" + (_buffCollectionSo.buffList[i] == null ||
                                                                        string.IsNullOrEmpty(_buffCollectionSo
                                                                            .buffList[i].BuffName)
                        ? "NULL"
                        : _buffCollectionSo.buffList[i].BuffName)), leftListButtonStyle))
                {
                    //选中                
                    if (_selectedBuffIndex == i) break;
                    _selectedBuffIndex = i;
                    //取消聚焦
                    GUIUtility.keyboardControl = 0;

                    //如果文件还没有创建，那么暂时还不创建
                    //等用户选择了他的类型后再创建
                    int index;
                    if (_buffCollectionSo.buffList[i] == null)
                    {
                        index = 0;
                    }
                    else
                    {
                        string t = _buffCollectionSo.buffList[i].GetType().Name;
                        index = Array.IndexOf(_subclassNames, t);
                        index = index < 0 ? 0 : index;
                        _currentBuffSo = new SerializedObject(_buffCollectionSo.buffList[i]);
                    }

                    //重置选中的类型
                    _lastSelectSubclassIndex = index;
                    _selectedSubclassIndex = index;
                    _changedSubClassSelection = false;
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndScrollView();


            GUILayout.BeginHorizontal();

            GUILayout.Label("最大Buff数量", GUILayout.Width(100f));

            _maxLengthBuffer = EditorGUILayout.IntField
                (_maxLengthBuffer, GUILayout.Width(50f));

            if (GUILayout.Button(new GUIContent("修改"), GUILayout.Width(60f)))
            {
                if (_maxLengthBuffer < _buffCollectionSo.Size)
                {
                    var set = EditorUtility.DisplayDialog("重设最大Buff数量",
                        "减少最大Buff数量会删除末尾的Buff，确认要修改吗？", "是", "否");

                    if (set)
                    {
                        _buffCollectionSo.ReSize(_maxLengthBuffer);
                        _selectedBuffIndex = -1;

                        DirectoryInfo directory = new DirectoryInfo(BuffsPath);
                        FileInfo[] files = directory.GetFiles("Buff*.asset");

                        foreach (FileInfo file in files)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(file.Name);
                            string fileIndexString = fileName.Substring(4).Split('.')[0];

                            int index = int.Parse(fileIndexString);
                            if (index >= _buffCollectionSo.Size)
                            {
                                AssetDatabase.DeleteAsset(Path.Combine(BuffsPath, file.Name));
                                Debug.Log("删除Buff：" + file.Name);
                            }
                        }
                    }
                }
                else _buffCollectionSo.ReSize(_maxLengthBuffer);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();


            if (_selectedBuffIndex < 0) return;

            //右侧
            Rect rightRect = new Rect(leftWidth, 0, position.width - leftWidth, position.height);
            GUILayout.BeginArea(rightRect, EditorStyles.helpBox);


            _selectedSubclassIndex =
                EditorGUILayout.Popup("Buff的类", _selectedSubclassIndex, _subclassNames);


            _changedSubClassSelection = _selectedSubclassIndex != _lastSelectSubclassIndex;
            _lastSelectSubclassIndex = _selectedSubclassIndex;

            if (_changedSubClassSelection)
            {
                //修改Buff类型
                //创建Buff asset资源的唯一位置
                if (!_applyToAll)
                    _buffCollectionSo.buffList[_selectedBuffIndex] = BuffInfo.CreateBuffInfo(
                        _selectedSubclassIndex == 0 ? PlaceholderBuffName : _subclassNames[_selectedSubclassIndex],
                        _selectedBuffIndex);
                else if (_applyToAll || _buffIdPrefix == 0)
                {
                    _buffCollectionSo.buffList[_selectedBuffIndex] = BuffInfo.CreateBuffInfo(
                        _selectedSubclassIndex == 0 ? PlaceholderBuffName : _subclassNames[_selectedSubclassIndex],
                        int.Parse($"{_buffIdPrefix}" + $"{_selectedBuffIndex}"));
                }

                CreateBuffAsset(_selectedBuffIndex);
                _currentBuffSo = new SerializedObject(_buffCollectionSo.buffList[_selectedBuffIndex]);
            }


            EditorGUILayout.Separator();
            // DrawDivider();
            //序列化类
            if (_selectedSubclassIndex != 0)
            {
                _currentBuffProp = _currentBuffSo.GetIterator();
                _currentBuffProp.NextVisible(true);
                _currentBuffSo.UpdateIfRequiredOrScript();
                while (_currentBuffProp.NextVisible(true))
                {
                    if (_currentBuffProp.name.Equals("buffTag"))
                    {
                        _currentBuffProp.enumValueFlag = (int)(BuffTag.BuffTag)EditorGUILayout.EnumFlagsField(
                            new GUIContent("Buff的Tag"),
                            (BuffTag.BuffTag)_currentBuffProp.enumValueFlag);
                    }
                    else if (_currentBuffProp.name.Equals("icon"))
                    {
                        _currentBuffProp.objectReferenceValue = EditorGUILayout.ObjectField("图标",
                            (Sprite)_currentBuffProp.objectReferenceValue, typeof(Sprite), false) as Sprite;
                    }
                    else if (_currentBuffProp.name.Equals("isPermanent"))
                    {
                        _currentBuffProp.NextVisible(true);
                    }
                    else
                        EditorGUILayout.PropertyField(_currentBuffProp, true);
                }

                if (GUI.changed)
                {
                    //Debug.Log("Saved");
                    _currentBuffSo.ApplyModifiedProperties();
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_buffCollectionSo.buffList[_selectedBuffIndex]);
                    EditorUtility.SetDirty(_buffCollectionSo);
                    AssetDatabase.SaveAssets();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新Buff", GUILayout.Width(60f))) Initialize();
            GUILayout.Space(50f);
            GUILayout.Label("BuffID前缀", GUILayout.Width(80f));
            _buffIdPrefix = EditorGUILayout.IntField(_buffIdPrefix, GUILayout.Width(50f));
            GUILayout.Label("应用到所有", EditorStyles.boldLabel, GUILayout.Width(80f));

            _applyToAll = EditorGUILayout.Toggle(_applyToAll, GUILayout.Width(20f));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
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
            _assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name.Equals(AssemblyName));
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
                ("t:BuffCollection", new[] { ScriptObjectPath });
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

        private void CreateBuffAsset(int index)
        {
            var fileName = "Buff" + $"{index:000}" + ".asset";

            var directory = Path.Combine(BuffsPath, fileName);
            if (File.Exists(directory)) File.Delete(directory);
            var buff = _buffCollectionSo.buffList[index];
            AssetDatabase.CreateAsset(buff, directory);
        }

        private void CreatBuffTagAsset()
        {
            var directory = Path.Combine(ScriptObjectPath.Replace("BuffData", "TagData"), DefaultTagSoName);
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