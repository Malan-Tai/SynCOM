using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunkerDown : BaseAllyAbility
{
    public override string GetDescription()
    {
        return "Increases the efficiency of your cover, if you have one.";
    }

    public override bool CanExecute()
    {
        return CombatGameManager.Instance.GridMap.GetBestCoverAt(_effector.GridPosition) != EnumCover.None;
    }

    protected override void EnemyTargetingInput()
    {
        // no target needed
    }

    public override void Execute()
    {
        AddBuff(_effector, new Buff("Hunkered Down", 2, _effector, 0, 0, 0, 0, 0, 2f));
        SendResultToHistoryConsole(null);
    }

    public override string GetName()
    {
        return "Hunker Down";
    }

    public override string GetShortDescription()
    {
        return "Increases cover";
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
            .AddText(_effector.Character.FirstName).CloseTag()
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" hunkered down ").CloseTag()
            .Submit();
    }

    public override void ShowRanges(AllyUnit user)
    {
    }
}
