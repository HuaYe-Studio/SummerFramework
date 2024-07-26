using UnityEngine;

namespace BattleFramework.BuffSystem.BuffBase
{
    public interface IBuffInfo
    {
        /// <summary>
        /// Buff启用时，生效前（即便该Buff不可作用于对象也会先执行）
        /// </summary>
        public void OnAwake();

        /// <summary>
        /// Buff开始生效时
        /// </summary>
        public void OnStart();

        public void OnRemove();
        public void OnBeDestroy();
        public void OnUpdate();

        /// <summary>
        /// Buff层数变化时
        /// </summary>
        public void OnModifyLayer(int change);

        /// <summary>
        /// 开始周期性效果，如果已经开启过(无论是否在之后停止了)，则重置计时器并重新开始
        /// </summary>
        /// <param name="interval">周期时间</param>
        public void StartTickEffect(float interval);

        /// <summary>
        /// 停止周期性效果
        /// </summary>
        public void StopTickEffect();

        /// <summary>
        /// 重置Buff以复用
        /// </summary>
        public void Reset();

        /// <summary>
        /// 重置Buff计时器
        /// </summary>
        public void ResetTimer();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="target">Buff施加的对象</param>
        /// <param name="caster">Buff施加者</param>
        public void Init(IBuffHandler target, GameObject caster);

        /// <summary>
        /// 让层数+/-=change
        /// </summary>
        /// <param name="change">改变的层数，可以为负数</param>
        public void ModifyLayer(int change);

        public void SetIsEnable(bool isEnable);
        public bool IsEmpty();
    }
}