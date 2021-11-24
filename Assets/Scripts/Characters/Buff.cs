using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Buffs are temporary traits that affect performannces in combat.
/// Note that debuffs are just buffs with negative values.
/// </summary>
public abstract class Buff : Trait
{
    private int _turnCount;
    private GridBasedUnit _ownerUnit;

    public Buff(int duration, GridBasedUnit unit)
    {
        _turnCount = duration;
        _ownerUnit = unit;

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
        /// Special effect ???
        /// 
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
}