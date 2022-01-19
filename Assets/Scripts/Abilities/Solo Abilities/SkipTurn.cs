using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class SkipTurn : BaseAllyAbility
{
    public override string GetDescription()
    {
        return "Do nothing and end your turn.";
    }

    public override string GetName()
    {
        return "Skip Turn";
    }

    public override bool CanExecute()
    {
        return true;
    }

    protected override void EnemyTargetingInput()
    {
        // no target needed
    }

    public override void Execute()
    {
        Debug.Log("[" + _effector.AllyCharacter.Name + "] SKIPS their turn");
        SendResultToHistoryConsole(null);
    }

    public override string GetShortDescription()
    {
        return "Skips turn";
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" skipped ").CloseTag()
            .AddText("his turn")
            .Submit();
    }
}

