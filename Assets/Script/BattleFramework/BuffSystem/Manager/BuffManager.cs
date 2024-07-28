using BattleFramework.BuffSystem.BuffBase;
using BattleFramework.BuffSystem.BuffTag;
using UnityEngine;
using Utility.LogSystem;
using Utility.SingletonPatternSystem;

namespace BattleFramework.BuffSystem.Manager
{
    public class BuffManager : MonoSingleton<BuffManager>, IBuffManager
    {
        [HideInInspector] [SerializeField] private BuffCollection _buffCollection;
        public bool IsWorking => _buffCollection != null;
        public IBuffTagManager TagManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (_buffCollection == null)
               Debug.Log("BuffCollection数据丢失");
        }

        public void SetData(BuffCollection buffCollection)
        {
            _buffCollection = buffCollection;
        }

        public IBuffInfo GetBuff(int id)
        {
            if (id < 0 || id >= _buffCollection.Size)
                LogSystem.Instance.Log($"使用非法的Buff id：{id} (当前Buff总数为{_buffCollection.Size})", LogLevelEnum.Error);
            if (_buffCollection.buffList[id] == null)
                LogSystem.Instance.Log($"引用的Buff为null。id：{id}", LogLevelEnum.Error);

            return _buffCollection.buffList[id].CloneBuff();
        }

        public void RegisterBuffTagManager(BuffTagManager tagManager)
        {
            TagManager = tagManager;
        }
    }
}