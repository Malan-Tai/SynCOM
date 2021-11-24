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
    private int _turnDuration;
    private int _turnCount;
    private GridBasedUnit _ownerUnit;

    public Buff(int duration, GridBasedUnit unit)
    {
        _turnDuration = duration;
        _turnCount = duration;
        _ownerUnit = unit;
    }

    public void Expire()
    {
        /// Special effect ???
        /// 
        this._ownerUnit.CurrentBuffs.Remove(this);
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        return baseSentiment;
    }

    public override string GetName()
    {
        return "";
    }

    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        return baseSentiment;
    }
}