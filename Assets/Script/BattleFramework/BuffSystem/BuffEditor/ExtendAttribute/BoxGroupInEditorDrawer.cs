#if UNITY_EDITOR
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BattleFramework.BuffSystem.Editor.ExtendAttribute
{
    [CustomPropertyDrawer(typeof(BoxGroupInEditorAttribute))]
    public class BoxGroupInEditorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 获取字段信息
            var fields = property.serializedObject.targetObject.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<BoxGroupAttribute>();
                if (attribute != null)
                {
                    // 绘制字段
                    if (field.FieldType == typeof(int))
                    {
                        field.SetValue(property.serializedObject.targetObject,
                            EditorGUILayout.IntField(field.Name,
                                (int)field.GetValue(property.serializedObject.targetObject)));
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(property.serializedObject.targetObject,
                            EditorGUILayout.TextField(field.Name,
                                (string)field.GetValue(property.serializedObject.targetObject)));
                    }
                    // 添加其他类型支持
                }
            }

            Draw($"{label}", property.serializedObject.targetObject);
        }

        public void Draw(string title, object target)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label(title, EditorStyles.boldLabel);

            // 获取字段信息
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<BoxGroupAttribute>();
                if (attribute != null)
                {
                    // 绘制字段
                    if (field.FieldType == typeof(int))
                    {
                        field.SetValue(target, EditorGUILayout.IntField(field.Name, (int)field.GetValue(target)));
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(target, EditorGUILayout.TextField(field.Name, (string)field.GetValue(target)));
                    }
                    // 添加其他类型支持
                }
            }

            GUILayout.EndVertical();
        }
    }
}
#endif