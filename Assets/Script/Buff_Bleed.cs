using BattleFramework.BuffSystem.BuffBase;
using UnityEngine;

public class Buff_Bleed : BuffInfo
{
    [SerializeField] private int bleedDamage;
    [SerializeField] private float bleedTimeInterval;
    // private Entity1 targetEntity;


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