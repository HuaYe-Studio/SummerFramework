using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleFramework.BuffSystem.BuffBase
{
    [CreateAssetMenu(fileName = "BuffCollection", menuName = "BuffCollection")]
    [Serializable]
    public class BuffCollection : ScriptableObject
    {
        [SerializeField] private int size = 20;

        [SerializeField] [ShowInInspector] public List<BuffInfo> buffList = new(20);
        public int Size => size;

        /// <summary>
        /// 修改最大Buff数量
        /// </summary>
        /// <param name="i"></param>
        public void ReSize(int i)
        {
            while (i < buffList.Count)
            {
                buffList.RemoveAt(buffList.Count - 1);
            }

            while (i > buffList.Count)
            {
                buffList.Add(BuffInfo.CreateBuffInfo("PlaceholderBuff", buffList.Count));
            }

            size = i;
        }

        /// <summary>
        /// 对齐最大Buff数量，用于在读入时初始化
        /// </summary>
        public void ReSize()
        {
            ReSize(size);
        }
    }
}