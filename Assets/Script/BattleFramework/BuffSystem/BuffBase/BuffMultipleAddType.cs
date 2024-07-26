namespace BattleFramework.BuffSystem.BuffBase
{
    /// <summary>
    /// Buff叠加方式
    /// </summary>
    public enum BuffMultipleAddType
    {
        ResetTime, //重置时间
        MultipleLayer, //叠加层数
        MultipleLayerAndResetTime, //叠加层数并重置时间
        MultipleCount, //多个同种Buff同时存在
    }
}