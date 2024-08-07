using System;
using BattleFramework.BuffSystem.BuffTag;
using UnityEditor;
using UnityEngine;

namespace BattleFramework.BuffSystem.Editor
{
    [CustomEditor(typeof(BitBuffTagData))]
    public class BitBuffTagEditor :UnityEditor.Editor
    {
        int _length;
        SerializedProperty _removedTagsData;
        SerializedProperty _blockTagsData;

        private void OnEnable()
        {
            _length = Enum.GetNames(typeof(BuffTag.BuffTag)).Length;
            _removedTagsData = serializedObject.FindProperty("removedTags");
            _blockTagsData = serializedObject.FindProperty("blockTags");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            _removedTagsData.arraySize = _length;
            _blockTagsData.arraySize = _length;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Tag"),
                GUILayout.MinWidth(0));
            EditorGUILayout.LabelField(new GUIContent("被该Tag抵消的"),
                GUILayout.MinWidth(0));
            EditorGUILayout.LabelField(new GUIContent("禁止该Tag添加的"),
                GUILayout.MinWidth(0));
            EditorGUILayout.EndHorizontal();
            for (int i = 1; i < _removedTagsData.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(((BuffTag.BuffTag)(1 << (i - 1))).ToString()),
                    GUILayout.MinWidth(0));
                //string name = tagNamesData.GetArrayElementAtIndex(i).stringValue;
                //tagNamesData.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(name);

                _removedTagsData.GetArrayElementAtIndex(i).intValue =
                    (int)(BuffTag.BuffTag)EditorGUILayout.EnumFlagsField((BuffTag.BuffTag)_removedTagsData
                        .GetArrayElementAtIndex(i)
                        .intValue);
                _blockTagsData.GetArrayElementAtIndex(i).intValue =
                    (int)(BuffTag.BuffTag)EditorGUILayout.EnumFlagsField((BuffTag.BuffTag)_blockTagsData
                        .GetArrayElementAtIndex(i)
                        .intValue);
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}