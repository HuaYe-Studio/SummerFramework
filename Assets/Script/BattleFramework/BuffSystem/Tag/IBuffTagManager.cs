namespace BattleFramework.BuffSystem.BuffTag
{
    public interface IBuffTagManager
    {
        /// <summary>
        /// 添加拥有tag的Buff时，是否会移除Tag为other的Buff
        /// </summary>
        /// <param name="btag">要添加的Buff的Tag</param>
        /// <param name="other">其他已有的Tag</param>
        /// <returns></returns>
        public bool RemoveOtherBuffWhenTagAdd(BuffTag btag, BuffTag other);

        /// <summary>
        /// 添加的拥有tag的Buff是否会被Tag为other的Buff抵消
        /// </summary>
        /// <param name="btag">要添加的Buff的Tag</param>
        /// <param name="other">其他已有的Tag</param>
        /// <returns></returns>
        public bool CanAddWhenOtherBuffExist(BuffTag btag, BuffTag other);
    }
}