using System;
using System.Diagnostics;
using UnityEngine;


namespace BattleFramework.BuffSystem.Editor.ExtendAttribute
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LabelTextInEditorAttribute : PropertyAttribute
    {
        public readonly string Label;

        public LabelTextInEditorAttribute(string label)
        {
            Label = label;
        }
    }
}