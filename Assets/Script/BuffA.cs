using BattleFramework.BuffSystem.BuffBase;
using Sirenix.OdinInspector;
using UnityEngine;

public class BuffA : BuffInfo
{
    [LabelText("攻击值")] [SerializeField] public int attackerValue;

    public override void OnStart()
    {
    }

    public override void OnRemove()
    {
    }

    public override void OnModifyLayer(int change)
    {
    }

    public override void Reset()
    {
    }

    protected override void OnBuffTickEffect()
    {
    }
}