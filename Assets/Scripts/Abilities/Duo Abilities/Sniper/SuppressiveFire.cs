using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuppressiveFire : BaseDuoAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;
    private AbilityStats _allyShotStats;

    public override string GetAllyDescription()
    {
        string res = "Launch a sneak attack on the distracted enemy, dealing critical damage.";
        if (_chosenAlly != null && _hoveredUnit != null)
        {
            res += "\nAcc:" + _allyShotStats.GetAccuracy(_hoveredUnit, _chosenAlly.LinesOfSight[_hoveredUnit].cover) + "%" + 
                    " | Crit: 100%" +
                    " | Dmg:" + _allyShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nAcc:" + _allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover) + "%" +
                    " | Crit: 100%" +
                    " | Dmg:" + _allyShotStats.GetDamage();
        }

        return res;
    }
    public override string GetDescription()
    {
        string res = "Shoot at a distant enemy to distract them.";
        if (_chosenAlly != null && _hoveredUnit != null)
        {
            res += "\nAcc:" + SniperAccuracy(_hoveredUnit) + "%" +
                    " | Crit:" + _selfShotStats.GetCritRate() + "%" +
                    " | Dmg:" + _selfShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nAcc:" + SniperAccuracy(target) + "%" +
                    " | Crit:" + _selfShotStats.GetCritRate() + "%" +
                    " | Dmg:" + _selfShotStats.GetDamage();
        }

        return res;
    }
    public override string GetName()
    {
        return "Suppressive Fire";
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
            // TODO:    Check if the unit can be targetted : must NOT be too close
            if (distanceToSelf > _effector.Character.RangeShot && distanceToAlly <= _chosenAlly.Character.RangeShot && _chosenAlly.LinesOfSight.ContainsKey(unit))
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

        _selfShotStats = new AbilityStats(0, 0, 1f, 0, 0, _effector);
        _allyShotStats = new AbilityStats(0, 9999, 2f, 0, 0, _chosenAlly);

        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);
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
            //SoundManager.PlaySound(SoundManager.Sound.RetentlessFoe);
        }

        else
        {
            //SoundManager.PlaySound(SoundManager.Sound.RetentlessNeutral);
        }
        Debug.Log("we are shooting at " + target.GridPosition + " with cover " + (int)_effector.LinesOfSight[target].cover);
        AbilityResult result = new AbilityResult();
        SoundManager.PlaySound(SoundManager.Sound.SuppressiveFire);
        ShootResult selfResult = SelfShoot(target, _selfShotStats);
        ShootResult allyResult = AllyShoot(target, _allyShotStats);

        result.Miss = !selfResult.Landed;
        result.Critical = selfResult.Critical;
        result.Damage = selfResult.Damage;
        result.AllyMiss = !allyResult.Landed;
        result.AllyCritical = allyResult.Critical;
        result.AllyDamage = allyResult.Damage;
        SendResultToHistoryConsole(result);
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];

        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
            .AddText(" and ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
            .AddText(" used ")
            .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .AddText(" on ")
            .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
            .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(target.Character.Name).CloseTag()
            .AddText(": ")
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag();

        if (result.Miss)
        {
            HistoryConsole.Instance.OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" missed ").CloseTag();
        }
        else
        {
            string selfCriticalText = result.Critical ? " critical" : "";
            HistoryConsole.Instance
                .AddText(" did ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage}{selfCriticalText} damage").CloseTag();
        }

        HistoryConsole.Instance
            .AddText(" and ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag();

        if (result.AllyMiss)
        {
            HistoryConsole.Instance.OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" missed").CloseTag();
        }
        else
        {
            string allyCriticalText = result.AllyCritical ? " critical" : "";
            HistoryConsole.Instance
                .AddText(" did ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.AllyDamage}{allyCriticalText} damage").CloseTag();
        }

        HistoryConsole.Instance.Submit();
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

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return true;
    }

    public override string GetShortDescription()
    {
        return "Distract a distant enemy while you ally land a critical hit";
    }

    /// <summary>
    /// Returns the accuracy of the Sniper. As they shoot outside of range, the farther away the target is the more accuracy they lose.
    /// </summary>
    private float SniperAccuracy(GridBasedUnit target)
    {
        float acc = _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover);

        float dist = (target.GridPosition - _effector.GridPosition).magnitude - _effector.AllyCharacter.RangeShot;

        float distPenalty = Mathf.Clamp01(dist / 10);

        acc = acc * (1 - distPenalty);

        return acc;
    }
}
