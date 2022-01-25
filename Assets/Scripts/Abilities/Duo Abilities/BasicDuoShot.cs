using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDuoShot : BaseDuoAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;
    private AbilityStats _allyShotStats;

    protected override void ChooseAlly()
    {
        _possibleTargets = new List<GridBasedUnit>();
        var tempTargets = new GridBasedUnit[_effector.LinesOfSight.Count];
        _effector.LinesOfSight.Keys.CopyTo(tempTargets, 0);

        foreach (GridBasedUnit unit in tempTargets)
        {
            float distanceToSelf = Vector2.Distance(unit.GridPosition, _effector.GridPosition);
            float distanceToAlly = Vector2.Distance(unit.GridPosition, _chosenAlly.GridPosition);
            if (distanceToSelf <= _effector.Character.RangeShot && distanceToAlly <= _chosenAlly.Character.RangeShot && _chosenAlly.LinesOfSight.ContainsKey(unit))
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

        _selfShotStats = new AbilityStats(0, 0, 1.5f, 0, 0, _effector);
        _allyShotStats = new AbilityStats(0, 0, 1.5f, 0, 0, _chosenAlly);

        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null && _targetIndex >= 0;
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

    public override void Execute()
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];
        Relationship relationshipAllyToSelf = _effector.AllyCharacter.Relationships[this._chosenAlly.AllyCharacter];
        
        if (relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Sympathy) < 0 || relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Admiration) < 0 || relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Trust) < 0)
        {
            SoundManager.PlaySound(SoundManager.Sound.RetentlessFoe);
        }
        else
        {
            SoundManager.PlaySound(SoundManager.Sound.RetentlessNeutral);
        }

        ShootResult selfResults = SelfShoot(target, _selfShotStats);
        ShootResult allyResults = AllyShoot(target, _allyShotStats);

        AbilityResult result = new AbilityResult();
        result.CopyShootResult(selfResults);
        result.CopyAllyShootResult(allyResults);
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

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return true;
    }

    public override string GetName()
    {
        return "Relentless Assault";
    }

    public override string GetDescription()
    {
        string res = "Take a shot at the target with augmented damage.";
        if (_chosenAlly != null && _hoveredUnit != null)
        {
            res += "\nAcc:" + _selfShotStats.GetAccuracy(_hoveredUnit, _effector.LinesOfSight[_hoveredUnit].cover) +
                    "% | Crit:" + _selfShotStats.GetCritRate() +
                    "% | Dmg:" + _selfShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nAcc:" + _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover) +
                    "% | Crit:" + _selfShotStats.GetCritRate() +
                    "% | Dmg:" + _selfShotStats.GetDamage();
        }

        return res;
    }

    public override string GetAllyDescription()
    {
        string res = "Take a shot at the target with augmented damage.";
        if (_chosenAlly != null && _hoveredUnit != null)
        {
            res += "\nAcc:" + _allyShotStats.GetAccuracy(_hoveredUnit, _chosenAlly.LinesOfSight[_hoveredUnit].cover) +
                    "% | Crit:" + _allyShotStats.GetCritRate() +
                    "% | Dmg:" + _allyShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nAcc:" + _allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover) +
                    "% | Crit:" + _allyShotStats.GetCritRate() +
                    "% | Dmg:" + _allyShotStats.GetDamage();
        }

        return res;
    }

    public override void UISelectUnit(GridBasedUnit unit)
    {
        if (_chosenAlly != null)
        {
            _targetIndex = _possibleTargets.IndexOf(unit);
            CombatGameManager.Instance.Camera.SwitchParenthood(unit);
            RequestDescriptionUpdate();
            RequestTargetSymbolUpdate(unit);
        }
        else base.UISelectUnit(unit);
    }

    public override string GetShortDescription()
    {
        return "A basic duo attack";
    }
}
