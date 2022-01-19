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
        Debug.Log("i hunker down");
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
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" hunkered down ").CloseTag()
            .Submit();
    }
}
