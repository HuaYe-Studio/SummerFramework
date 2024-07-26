using UnityEngine;
using System;

namespace BattleFramework.BuffSystem
{
    public interface IBuffHandler
    {
        /// <summary>
        /// 添加Buff
        /// </summary>
        /// <param name="buffId">Buff的ID</param>
        /// <param name="caster">Buff的施加者</param>
        public void AddBuff(int buffId, GameObject caster);

        /// <summary>
        /// 移除Buff
        /// </summary>
        /// <param name="buffId">要移除的Buff的ID</param>
        /// <param name="removeAll">如果拥有多个同ID的Buff，是否全部移除</param>
        public void RemoveBuff(int buffId, bool removeAll = true);

        /// <summary>
        /// 移除Buff，不触发OnRemove
        /// </summary>
        /// <param name="buffId">要移除的Buff的ID</param>
        /// <param name="interruptAll">如果拥有多个同ID的Buff，是否全部移除</param>
        public void InterruptBuff(int buffId, bool interruptAll = true);

        /// <summary>
        /// 注册事件：添加Buff时
        /// </summary>
        /// <param name="act"></param>
        public void RegisterOnAddBuff(Action act);

        /// <summary>
        /// 取消注册事件：添加Buff时
        /// </summary>
        /// <param name="act"></param>
        public void UnregisterOnAddBuff(Action act);

        /// <summary>
        /// 注册事件：移除Buff时
        /// </summary>
        /// <param name="act"></param>
        public void RegisterOnRemoveBuff(Action act);

        /// <summary>
        /// 取消注册事件：移除Buff时
        /// </summary>
        /// <param name="act"></param>
        public void UnregisterOnRemoveBuff(Action act);
    }
}