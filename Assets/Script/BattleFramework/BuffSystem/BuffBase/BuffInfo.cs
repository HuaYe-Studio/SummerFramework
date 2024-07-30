using System;
using BattleFramework.BuffSystem.BuffHandler;
using Sirenix.OdinInspector;
using UnityEngine;
using BattleFramework.BuffSystem;
using BattleFramework.BuffSystem.Editor.ExtendAttribute;

namespace BattleFramework.BuffSystem.BuffBase
{
    [Serializable]
    public abstract class BuffInfo : ScriptableObject, IBuffInfo, IComparable<BuffInfo>

    {
        #region Func

        public static BuffInfo CreateBuffInfo(string thisName, int thisId)
        {
            var buffInfo = (BuffInfo)CreateInstance(thisName);
            buffInfo.id = thisId;
            return buffInfo;
        }

        public BuffInfo CloneBuff()
        {
            return Instantiate(this);
        }

        #endregion

        #region 基本信息

        [BoxGroup("基本信息")] [LabelTextInEditor("图标")] [LabelText("图标")] [SerializeField]
        private Sprite icon;

        [LabelTextInEditor("Buff名")] [LabelText("Buff名")] [SerializeField]
        private string buffName;

        [LabelTextInEditor("Buff的ID")] [LabelText("Buff的ID")] [SerializeField]
        private int id;

        [LabelTextInEditor("Buff描述")] [LabelText("Buff描述")] [SerializeField]
        private string description;

        public BuffHandler.BuffHandler Target { get; private set; }
        public GameObject Caster { get; private set; }
        public int Layer { get; private set; }


        [BoxGroup("逻辑相关")] [EnumPaging] [LabelTextInEditor("重复添加方式")] [LabelText("重复添加方式")] [SerializeField]
        private BuffMultipleAddType multipleAddType;

        private bool _isEnable;

        [LabelTextInEditor("倒计时结束时层数-1")] [LabelText("倒计时结束时层数-1")] [SerializeField]
        private bool removeOneLayerOnTimeUp;


        [LabelTextInEditor("Buff的Tag")] [LabelText("Buff的Tag")] [SerializeField]
        private BuffTag.BuffTag buffTag;

        #region Buff时间相关

        [BoxGroup("Buff时间相关")] private float _timer;

        [LabelTextInEditor("是否永久")] [LabelText("是否永久")] [SerializeField]
        private bool isPermanent;

        [LabelTextInEditor("持续时间")] [LabelText("持续时间")] [SerializeField]
        private float duration;

        //周期定时效果
        private float _tickTimer;
        private float _tickInterval;
        private bool _isTickEffectEnable = false;

        public float RemainingTime => _timer; //剩余时间
        public float Duration => duration; //总时间
        public float TickRemainTime => _tickTimer; //周期时间
        public bool IsPermanent => isPermanent;

        #endregion

        #endregion

        #region Public

        public string BuffName => buffName;
        public string Description => description;

        public int ID => id;


        public Sprite Icon => icon;
        public BuffTag.BuffTag BuffTag => buffTag;
        public BuffMultipleAddType MultipleAddType => multipleAddType;
        public bool RemoveOneLayerOnTimeUp => removeOneLayerOnTimeUp;
        public bool IsEnable => _isEnable;

        #endregion

        #region Private

        private int _temporaryLayer;
        private bool _layerModified;
        private bool _firstFrame;

        #endregion


        #region 生命周期

        public virtual void OnAwake()
        {
            _timer = duration;
            _isEnable = true;
            Layer = 0;
            ModifyLayer(1);
            _firstFrame = true;
        }

        public abstract void OnStart();


        public abstract void OnRemove();

        public virtual void OnBeDestroy()
        {
            if (multipleAddType is BuffMultipleAddType.MultipleLayer
                or BuffMultipleAddType.MultipleLayerAndResetTime)
            {
                ModifyLayer(-Layer);
            }

            ModifyRealLayer();
        }


        public void OnUpdate()
        {
            if (_firstFrame)
            {
                _firstFrame = false;
                return;
            }

            if (!_isEnable) return;
            if (!isPermanent)
            {
                _timer -= Time.deltaTime;
                while (_timer <= 0 && _isEnable)
                {
                    if (removeOneLayerOnTimeUp)
                    {
                        _timer += duration;
                        ModifyLayer(-1);
                    }
                    else
                    {
                        _isEnable = false;
                        _timer = 0;
                    }
                }
            }

            ModifyRealLayer();
            if (!_isTickEffectEnable) return;
            _tickTimer -= Time.deltaTime;
            while (_tickTimer <= 0)
            {
                _tickTimer += _tickInterval;
                OnBuffTickEffect();
            }
        }

        public abstract void OnModifyLayer(int change);
        protected abstract void OnBuffTickEffect();

        public void StartTickEffect(float interval)
        {
            _isTickEffectEnable = true;
            _tickInterval = interval;
            _tickTimer = interval;
        }

        public void StopTickEffect()
        {
            _isTickEffectEnable = false;
        }

        public abstract void Reset();

        public void ResetTimer()
        {
            _timer = duration;
        }

        public void Init(IBuffHandler target, GameObject caster)
        {
            Target = (BuffHandler.BuffHandler)target;
            Caster = caster;
        }

        public void ModifyLayer(int change)
        {
            if (Layer + change != 0 && Layer + change != 1 &&
                multipleAddType != BuffMultipleAddType.MultipleLayer &&
                multipleAddType != BuffMultipleAddType.MultipleLayerAndResetTime)
                return;
            _temporaryLayer += change;
            _layerModified = true;
        }

        public void ModifyRealLayer()
        {
            Layer += _temporaryLayer;
            if (_layerModified) OnModifyLayer(Layer < 0 ? -Layer : _temporaryLayer);
            if (Layer <= 0) _isEnable = false;
            _temporaryLayer = 0;
            _layerModified = false;
        }

        public void SetIsEnable(bool isEnable)
        {
            _isEnable = isEnable;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(buffName);
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return string.Concat(ID, ". ", buffName, ":", _timer, "/", duration, "\tEffective: ", _isEnable);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, base.GetHashCode());
        }

        public override bool Equals(object other)
        {
            if (other is not BuffInfo info) return false;
            return ID == info.ID;
        }

        public int CompareTo(BuffInfo other)
        {
            return ID.CompareTo(other.ID);
        }

        #endregion
    }
}