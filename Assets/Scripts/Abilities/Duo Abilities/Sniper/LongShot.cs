using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongShot : BaseDuoAbility
{
    private AbilityStats _selfShotStats;
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;
    private int _selectionRange = 8;
    public override string GetName()
    {
        return "Long Shot";
    }

    public override string GetShortDescription()
    {
        return "Shoot a distant enemy indicated by your ally.";
    }

    public override string GetDescription()
    {
        string res = "Shoot a distant enemy indicated by your ally. Not seeing it coming, the enemy takes increased damage.";

        // The enemy cover used is the one of the _chosenAlly (don't worry there's a scientific explanation)
        if (_chosenAlly != null && _hoveredUnit != null)
        {
            res += "\nAcc:" + _selfShotStats.GetAccuracy(_hoveredUnit, _chosenAlly.LinesOfSight[_hoveredUnit].cover) +
                    "% | Crit:" + _selfShotStats.GetCritRate() +
                    "% | Dmg:" + _selfShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nAcc:" + _selfShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover) +
                    "% | Crit:" + _selfShotStats.GetCritRate() +
                    "% | Dmg:" + _selfShotStats.GetDamage();
        }

        return res;
    }

    public override string GetAllyDescription()
    {
        return "Indicate the position of an enemy to the Sniper, allowing them to shoot it even if they're out of range.";
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - _effector.GridPosition).magnitude <= _selectionRange;
    }

    protected override void ChooseAlly()
    {
        _possibleTargets = new List<GridBasedUnit>();
        var tempTargets = new GridBasedUnit[_effector.LinesOfSight.Count];
        _effector.LinesOfSight.Keys.CopyTo(tempTargets, 0);

        foreach (GridBasedUnit unit in tempTargets)
        {
            float distanceToSelf = Vector2.Distance(unit.GridPosition, _effector.GridPosition);
            float distanceToAlly = Vector2.Distance(unit.GridPosition, _chosenAlly.GridPosition);
            if (distanceToSelf <= _effector.Character.RangeShot && distanceToAlly <= _effector.Character.RangeShot && _chosenAlly.LinesOfSight.ContainsKey(unit))
            {
                _possibleTargets.Add(unit);
            }
        }

        RequestTargetsUpdate(_possibleTargets);

        if (_possibleTargets.Count > 0)
        {
            _targetIndex = 0;
            CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
            RequestTargetSymbolUpdate(_possibleTargets[_targetIndex]);
        }
        else RequestTargetSymbolUpdate(null);

        _selfShotStats = new AbilityStats(0, 0, 2f, 0, 0, _effector);

        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);
    }

    protected override void EnemyTargetingInput()
    {
        if (_possibleTargets.Count <= 0) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        bool changedUnitThisFrame = false;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitUnit = hitData.transform.GetComponent<EnemyUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked)
            {
                int newIndex = _possibleTargets.IndexOf(hitUnit);
                if (newIndex >= 0)
                {
                    _targetIndex = newIndex;
                    changedUnitThisFrame = true;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !changedUnitThisFrame)
        {
            _targetIndex++;
            if (_targetIndex >= _possibleTargets.Count) _targetIndex = 0;
            changedUnitThisFrame = true;
        }

        if (changedUnitThisFrame)
        {
            CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
            RequestDescriptionUpdate();
            RequestTargetSymbolUpdate(_possibleTargets[_targetIndex]);
        }
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null && _targetIndex >= 0;
    }

    public override void Execute()
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];
        Relationship relationshipAllyToSelf = _effector.AllyCharacter.Relationships[this._chosenAlly.AllyCharacter];

        if (relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Sympathy) < 0 || relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Admiration) < 0 || relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Trust) < 0)
        {
            Debug.Log("OUI BINJOUR");
            //SoundManager.PlaySound(SoundManager.Sound.RetentlessFoe);
        }

        else
        {
            Debug.Log("NON");
            //SoundManager.PlaySound(SoundManager.Sound.RetentlessNeutral);
        }

        AbilityResult result = new AbilityResult();
        ShootResult selfResults = SelfShoot(target, _selfShotStats);
        result.CopyShootResult(selfResults);
        SendResultToHistoryConsole(result);
    }

    /// <summary>
    /// Literally the same as SelfShoot, but use the enemy cover seen from _chosenAlly instead of the _effector's.
    /// </summary>
    protected override ShootResult SelfShoot(GridBasedUnit target, AbilityStats selfShotStats, bool alwaysHit = false, bool canCrit = true)
    {
        if (StartAction(ActionTypes.Attack, _effector, _chosenAlly))
        {
            return new ShootResult(true, false, 0f, false);
        }

        int randShot = RandomEngine.Instance.Range(0, 100); // between 0 and 99
        int randCrit = RandomEngine.Instance.Range(0, 100);

        if (alwaysHit || randShot < selfShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover))
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, true, _chosenAlly);

            if (canCrit && randCrit < selfShotStats.GetCritRate())
            {
                AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage() * 1.5f, true, _chosenAlly);
                return new ShootResult(false, true, selfShotStats.GetDamage() * 1.5f, true);
            }
            else
            {
                AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage(), false, _chosenAlly);
                return new ShootResult(false, true, selfShotStats.GetDamage(), false);
            }
        }
        else
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, false, _chosenAlly);
            return new ShootResult(false, false, 0f, false);
        }
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];

        if (result.Cancelled)
        {

            HistoryConsole.Instance
                .BeginEntry()
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
                .AddText(" cancelled ")
                .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
                .AddText(" with ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
                .AddText(" to do something else...");
        }
        else
        {
            HistoryConsole.Instance
                .BeginEntry()
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
                .AddText(" indicated the position of ")
                .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
                .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(target.Character.Name).CloseTag()
                .AddText(" so that ")
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
                .AddText(" can use ")
                .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag();

            if (result.Miss)
            {
                HistoryConsole.Instance
                    .AddText(": he ")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText("missed").CloseTag();
            }
            else
            {
                string criticalText = result.Critical ? " critical" : "";

                HistoryConsole.Instance
                    .AddText(": he did ")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage}{criticalText} damage").CloseTag();
            }
        }

        HistoryConsole.Instance.Submit();
    }
}
