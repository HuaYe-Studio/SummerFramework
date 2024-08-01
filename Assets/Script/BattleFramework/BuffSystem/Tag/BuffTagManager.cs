using BattleFramework.BuffSystem.Manager;
using UnityEngine;
using Utility.SingletonPatternSystem;

namespace BattleFramework.BuffSystem.BuffTag
{
    public abstract class BuffTagManager : MonoSingleton<BuffTagManager>, IBuffTagManager
    {
        public abstract bool RemoveOtherBuffWhenTagAdd(BuffTag btag, BuffTag other);

        public abstract bool CanAddWhenOtherBuffExist(BuffTag btag, BuffTag other);

        protected virtual void Start()
        {
            BuffManager.Instance.RegisterBuffTagManager(this);
        }
    }
}