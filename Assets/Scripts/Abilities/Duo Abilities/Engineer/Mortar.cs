using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mortar : BaseDuoAbility
{
    private List<Tile> _areaOfEffectTiles = new List<Tile>();
    private List<EnemyUnit> _targets = new List<EnemyUnit>();
    private List<AllyUnit> _allyTargets = new List<AllyUnit>();

    private AbilityStats _selfShotStats;

    private int _radius = 3;

    public override string GetAllyDescription()
    {
        return "Send a beacon in the air to indicate your position. You take cover but have a small chance to get hit." +
               "\nCHANCE:10%";
    }

    public override string GetDescription()
    {
        string res = "Fire a splinter-filled grenade on you ally's position, hoping they’ll take cover in time.";
        if (_chosenAlly != null)
        {
            res += "\nACC:100%" +
                    " | CRIT:0%" +
                    " | DMG:" + (int)_selfShotStats.GetDamage();
        }
        else if (_effector != null & _temporaryChosenAlly != null)
        {
            var temporarySelfShotStat = new AbilityStats(0, 0, 1.5f, 0, 0, _effector);
            temporarySelfShotStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nACC:100%" +
                    " | CRIT:0%" +
                    " | DMG:" + (int)temporarySelfShotStat.GetDamage();
        }
        else
        {
            res += "\nACC:100%" +
                    " | CRIT:0%";
        }
        return res;
    }
    public override string GetName()
    {
        return "Mortar";
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    protected override void ChooseAlly()
    {
        _selfShotStats = new AbilityStats(0, 0, 1.5f, 0, 0, _effector);
        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);

        _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_chosenAlly.GridPosition, _radius);
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("DamageZone", _areaOfEffectTiles, false);

        // Je cache le highlight des anciennes targets
        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            enemy.DontHighlightUnit();
        }
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            ally.DontHighlightUnit();
        }

        _targets.Clear();
        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            if (Mathf.Abs(enemy.GridPosition.x - _chosenAlly.GridPosition.x) + Mathf.Abs(enemy.GridPosition.y - _chosenAlly.GridPosition.y) <= _radius)
            {
                _targets.Add(enemy);
                enemy.HighlightUnit(Color.red);
            }
        }
        _allyTargets.Clear();
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            if (Mathf.Abs(ally.GridPosition.x - _chosenAlly.GridPosition.x) + Mathf.Abs(ally.GridPosition.y - _chosenAlly.GridPosition.y) <= _radius)
            {
                _allyTargets.Add(ally);
                ally.HighlightUnit(Color.red);
            }
        }

        //CombatGameManager.Instance.Camera.SwitchParenthood(_chosenAlly);
    }

    protected override void EnemyTargetingInput()
    {

    }

    public override void Execute()
    {
        _allyTargets.Remove(_chosenAlly);
        AbilityResult result = new AbilityResult();
        SoundManager.PlaySound(SoundManager.Sound.Mortar);
        // Only the _chosenAlly knows the attack is incomming and (almost) always take cover
        if (RandomEngine.Instance.Range(0, 100) > 90)
        {
            Debug.Log("[Mortar] Ally didn't take cover in time");
            _allyTargets.Add(_chosenAlly);
        }

        foreach (EnemyUnit target in _targets)
        {
            result.DamageList.Add(AttackDamage(_effector, target, _selfShotStats.GetDamage(), false));
        }
        foreach (AllyUnit ally in _allyTargets)
        {
            result.DamageList.Add(FriendlyFireDamage(_effector, ally, _selfShotStats.GetDamage(), ally));
        }

        SendResultToHistoryConsole(result);
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
            .AddText(_effector.Character.FirstName).CloseTag()
            .AddText(" and ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
            .AddText(_chosenAlly.Character.FirstName).CloseTag()
            .AddText(" used ")
            .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag();

        List<GridBasedUnit> everyTarget = new List<GridBasedUnit>();
        everyTarget.AddRange(_targets);
        everyTarget.AddRange(_allyTargets);

        if (everyTarget.Count == 0)
        {
            HistoryConsole.Instance.AddText(": it damaged no one");
        }
        else
        {
            HistoryConsole.Instance.AddText(": did ");
        }

        for (int i = 0; i < everyTarget.Count; i++)
        {
            if (i != 0)
            {
                if (i == everyTarget.Count - 1)
                {
                    HistoryConsole.Instance.AddText(" and ");
                }
                else
                {
                    HistoryConsole.Instance.AddText(", ");
                }
            }

            HistoryConsole.Instance
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(result.DamageList[i].ToString()).CloseTag()
                .AddText(" to ")
                .OpenLinkTag(everyTarget[i].Character.Name, everyTarget[i], EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(everyTarget[i].Character.FirstName).CloseTag();
        }

        HistoryConsole.Instance.Submit();
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return true;
    }

    public override string GetShortDescription()
    {
        return "Fires a grenade to a beacon thrown by an ally, who has a chance to be hit.";
    }

    protected override void EndAbility()
    {
        base.EndAbility();

        // Je cache le highlight des anciennes targets
        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            enemy.DontHighlightUnit();
        }
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            ally.DontHighlightUnit();
        }

        CombatGameManager.Instance.TileDisplay.HideTileZone("DamageZone");
    }

    public override void ShowRanges(AllyUnit user)
    {
    }
}
