using System;
using UnityEngine;
using Utility.LogSystem;

namespace BattleFramework.BuffSystem.BuffTag
{
    public class BitBuffTagManager : BuffTagManager
    {
        [HideInInspector, SerializeField] private BitBuffTagData tagData;

        public void SetData(BitBuffTagData data)
        {
            tagData = data;
        }

        private void Awake()
        {
            if (tagData == null)
                LogSystem.Instance.Log("Tag数据丢失", LogLevelEnum.Error);
        }

        public override bool RemoveOtherBuffWhenTagAdd(BuffTag btag, BuffTag other)
        {
            switch (btag)
            {
                case 0:
                    return false;
                case < 0:
                    throw new Exception("使用了负标签");
                default:
                {
                    var index = BitBuffTagData.GetIndex(btag);
                    return (tagData.RemovedTags[index] & (int)other) > 0;
                }
            }
        }

        public override bool CanAddWhenOtherBuffExist(BuffTag btag, BuffTag other)
        {
            switch (btag)
            {
                case 0:
                    return false;
                case < 0:
                    throw new Exception("使用了负标签");
                default:
                {
                    var index = BitBuffTagData.GetIndex(btag);
                    return (tagData.BlockTags[index] & (int)other) > 0;
                }
            }
        }
    }
}