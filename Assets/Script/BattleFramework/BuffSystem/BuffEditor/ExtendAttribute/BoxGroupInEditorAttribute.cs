using System;
using System.Diagnostics;
using UnityEngine;

namespace BattleFramework.BuffSystem.Editor.ExtendAttribute
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.All)]
    public class BoxGroupInEditorAttribute : PropertyAttribute
    {
        public string Title { get; }

        public BoxGroupInEditorAttribute(string title)
        {
            Title = title;
        }
    }
}