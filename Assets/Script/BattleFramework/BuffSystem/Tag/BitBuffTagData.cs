using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleFramework.BuffSystem.BuffTag
{
    /// <summary>
    /// 存储位Tag数据的ScriptableObject
    /// </summary>
    [Serializable]
    public class BitBuffTagData : BuffTagData
    {
        [Tooltip("拥有这些Tag的旧Buff会被移除")] public List<int> removedTags;

        [Tooltip("存在拥有这些Tag的Buff时，新Buff无法被添加")]
        public List<int> blockTags;

        public int[] RemovedTags => removedTags.ToArray();

        public int[] BlockTags => blockTags.ToArray();


        public static int GetIndex(BuffTag buffTag)
        {
            if (buffTag == 0) return 0;
            var i = 1;
            while (((int)buffTag & (1 << i)) == 0)
                i++;
            return i + 1;
        }

        public void Init()
        {
            removedTags = Enumerable.Repeat(0, 32).ToList();
            blockTags = Enumerable.Repeat(0, 32).ToList();
        }
    }
}