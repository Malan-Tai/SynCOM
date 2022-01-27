using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Duo Ability <para/>
/// Heals an adjacent ally by a fixed amount
/// </summary>
public class FirstAid : BaseDuoAbility
{
    private AbilityStats _healStats;

    protected override void ChooseAlly()
    {
        _ignoreEnemyTargeting = true;
        _healStats = new AbilityStats(0, 0, 0, 0, 5, _effector);
        _healStats.UpdateWithEmotionModifiers(_chosenAlly);
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null; // && _chosenAlly.HP > 0
    }

    protected override void EnemyTargetingInput()
    {

    }

    public override void Execute()
    {
        SoundManager.PlaySound(SoundManager.Sound.FirstAid);
        AbilityResult result = new AbilityResult();
        result.Heal = Heal(_effector, _chosenAlly, _healStats.GetHeal(), _chosenAlly);

        SendResultToHistoryConsole(result);
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
            .AddText(" used ")
            .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" to heal ").CloseTag()
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
            .AddText(" for ")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Heal} health points").CloseTag()
            .Submit();
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude < 2;
    }

    public override string GetName()
    {
        return "First Aid";
    }

    public override string GetAllyDescription()
    {
        return "Your wounds are healed.";
    }

    public override string GetDescription()
    {
        string res = "You heal an injured ally.";

        if (_chosenAlly != null)
        {
            res += "\nHEAL:" + (int)_healStats.GetHeal();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporaryHealStat = new AbilityStats(0, 0, 0, 0, 5, _effector);
            temporaryHealStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nHEAL:" + (int)temporaryHealStat.GetHeal();
        }
        else
        {
            res += "\nHEAL:5";
        }
        return res;
    }

    public override string GetShortDescription()
    {
        return "A small heal";
    }

    public override void ShowRanges(AllyUnit user)
    {
        GridMap map = CombatGameManager.Instance.GridMap;
        List<Tile> range = new List<Tile>();

        for (int i = 0; i < map.GridTileWidth; i++)
        {
            for (int j = 0; j < map.GridTileHeight; j++)
            {
                Vector2Int tile = new Vector2Int(i, j);
                if ((tile - user.GridPosition).magnitude < 2)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
