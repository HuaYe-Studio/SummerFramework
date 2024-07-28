// using UnityEngine;
// using Sirenix.OdinInspector.Editor;
// using UnityEditor;
// using System.IO;
// using BattleFramework.BuffSystem.BuffBase;
// using Utility.LogSystem;
//
// namespace NoSLoofah.BuffSystem.Editor
// {
//     public class BuffEditor : OdinEditorWindow
//     {
//         [MenuItem("Tools/BuffEditor")]
//         public static void OpenWindow()
//         {
//             GetWindow<BuffEditor>().Show();
//         }
//
//         private OdinMenuTree menuTree;
//         private BuffCollection SO;
//         private static readonly string defaultSOName = "BuffCollection.asset";
//
//         private static string SO_PATH =>
//             Path.Combine(Application.dataPath,
//                 "Script", "BattleFramework", "BuffSystem", "Data", "BuffData");
//
//         private void OnEnable()
//         {
//             Initialize();
//             if (Directory.Exists(SO_PATH))
//             {
//                 Debug.Log("1223");
//             }
//
//             LogSystem.Instance.Log(SO_PATH, LogLevelEnum.Debug);
//         }
//
//         private void Initialize()
//         {
//             menuTree = new OdinMenuTree { { "Buffs", null } };
//             LoadBuffs();
//         }
//
//         private void LoadBuffs()
//         {
//             string[] assetPaths = AssetDatabase.FindAssets
//                 ("BuffCollection t:BuffCollection");
//             if (assetPaths.Length > 0)
//             {
//                 string assetPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
//                 SO = AssetDatabase.LoadAssetAtPath<BuffCollection>(assetPath);
//                 SO.ReSize();
//                 for (int i = 0; i < SO.Size; i++)
//                 {
//                     var buff = SO.buffList[i];
//                     menuTree.Add($"Buff {i:000}: {buff?.BuffName ?? "NULL"}", buff);
//                 }
//             }
//         }
//
//         protected override void OnGUI()
//         {
//             menuTree.DrawMenuTree();
//             if (menuTree.Selection.SelectedValue != null)
//             {
//                 DrawBuffDetails(menuTree.Selection.SelectedValue as BuffInfo);
//             }
//         }
//
//         private void DrawBuffDetails(BuffInfo selectedBuff)
//         {
//             if (selectedBuff == null) return;
//
//             var serializedObject = new SerializedObject(selectedBuff);
//             serializedObject.Update();
//
//             EditorGUILayout.LabelField("Buff Details", EditorStyles.boldLabel);
//             EditorGUILayout.Space();
//
//             var property = serializedObject.GetIterator();
//             while (property.NextVisible(true))
//             {
//                 EditorGUILayout.PropertyField(property, true);
//             }
//
//             if (GUILayout.Button("Save Changes"))
//             {
//                 serializedObject.ApplyModifiedProperties();
//                 EditorUtility.SetDirty(selectedBuff);
//                 AssetDatabase.SaveAssets();
//             }
//         }
//     }
// }