using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PepTalk : BaseDuoAbility
{
    public override string GetDescription()
    {
        return "Your words of encouragement strengthen you both, increasing damage, move and aim for the next 2 turn." +
               "\nDMG +20% | ACC +50% | MOVE +3";
    }

    public override string GetShortDescription()
    {
        return "A small stat buff to you and your ally.";
    }

    public override string GetAllyDescription()
    {
        return "You ally's words of encouragement strengthen you, increasing damage, move and aim for the next 2 turn." +
               "\nDMG +20% | ACC +50% | MOVE +3";
    }
    public override string GetName()
    {
        return "Pep Talk";
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    protected override void ChooseAlly()
    {
        _ignoreEnemyTargeting = true;
    }

    protected override void EnemyTargetingInput()
    {
        
    }

    public override void Execute()
    {
        AddBuff(_effector, new Buff("Pumped", duration: 6, _effector, moveBuff: 3, damageBuff: 0.2f, accuracyBuff: 0.5f));
        AddBuff(_chosenAlly, new Buff("Pumped", duration: 6, _effector, moveBuff: 3, damageBuff: 0.2f, accuracyBuff: 0.5f));

        SendResultToHistoryConsole(null);
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
            .AddText(_effector.Character.FirstName).CloseTag()
            .AddText(" used ")
            .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .AddText(" to strengthen ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
            .AddText(_chosenAlly.Character.FirstName).CloseTag()
            .AddText(" and himself for the next ")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText("2 turns").CloseTag()
            .AddText(": gave ")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText("DMG +20%").CloseTag()
            .AddText(" | ")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText("ACC +50%").CloseTag()
            .AddText(" | ")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText("MOVE +3").CloseTag()
            .Submit();
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= 5;
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
                if ((tile - user.GridPosition).magnitude <= 5)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
