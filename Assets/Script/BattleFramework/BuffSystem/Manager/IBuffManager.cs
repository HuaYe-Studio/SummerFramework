using BattleFramework.BuffSystem.BuffBase;
using BattleFramework.BuffSystem.BuffTag;

namespace BattleFramework.BuffSystem.Manager
{
    public interface IBuffManager
    {
        /// <summary>
        /// 通过Id获取Buff
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IBuffInfo GetBuff(int id);

        public void RegisterBuffTagManager(BuffTagManager mgr);
    }
}