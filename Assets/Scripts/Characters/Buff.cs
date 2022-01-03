using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Buffs are temporary traits that affect performannces in combat.
/// Note that debuffs are just buffs with negative values.
/// </summary>
public class Buff : Trait
{
    private int _turnCount;
    private GridBasedUnit _ownerUnit;

    private float _damageBuff;
    private float _critBuff;
    private float _accuracyBuff;
    private float _mitigationBuff;
    private float _moveBuff;
    private float _dodgeBuff;

    public Buff(
        int duration,
        GridBasedUnit unit,
        float damageBuff = 0,
        float critBuff = 0,
        float accuracyBuff = 0,
        float mitigationBuff = 0,
        float moveBuff = 0,
        float dodgeBuff = 0
        )
    {
        _turnCount = duration;
        _ownerUnit = unit;

        _damageBuff = damageBuff;
        _critBuff = critBuff;
        _accuracyBuff = accuracyBuff;
        _mitigationBuff = mitigationBuff;
        _moveBuff = moveBuff;
        _dodgeBuff = dodgeBuff;

        CombatGameManager.OnNewTurn += HandleNewTurn;
    }

    public override string GetName()
    {
        return "";
    }

    private void HandleNewTurn()
    {
        _turnCount--;
        if (_turnCount <= 0)
        {
            Expire();
        }
    }

    public virtual void Expire()
    {
        /// override in derived classes to implement special effects on expiration

        CombatGameManager.OnNewTurn -= HandleNewTurn;
        this._ownerUnit.CurrentBuffs.Remove(this);
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        return baseSentiment;
    }

    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        return baseSentiment;
    }

    public override float GetDamageModifier()
    {
        return _damageBuff;
    }

    public override float GetCritRateModifier()
    {
        return _critBuff;
    }

    public override float GetHitRateModifier()
    {
        return _accuracyBuff;
    }

    public override float GetDodgeModifier()
    {
        return _dodgeBuff;
    }

    public override float GetMitigationModifier()
    {
        return _mitigationBuff;
    }

    public override float GetMoveModifier()
    {
        return _moveBuff;
    }

    public override string GetDescription()
    {
        return "";
    }
}