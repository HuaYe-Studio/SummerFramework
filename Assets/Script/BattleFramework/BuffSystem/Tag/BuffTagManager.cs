using BattleFramework.BuffSystem.Manager;
using UnityEngine;

namespace BattleFramework.BuffSystem.BuffTag
{
    public abstract class BuffTagManager : MonoBehaviour, IBuffTagManager
    {
        public abstract bool RemoveOtherBuffWhenTagAdd(BuffTag btag, BuffTag other);

        public abstract bool CanAddWhenOtherBuffExist(BuffTag btag, BuffTag other);

        protected virtual void Start()
        {
            BuffManager.Instance.RegisterBuffTagManager(this);
        }
    }
}