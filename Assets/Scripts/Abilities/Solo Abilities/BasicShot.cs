using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BasicShot : BaseAllyAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;

    private List<Tile> _possibleTargetsTiles = new List<Tile>();

    public override string GetName()
    {
        return "Basic Attack";
    }
    public override string GetShortDescription()
    {
        return "A basic attack";
    }

    public override string GetDescription()
    {
        string res = "Shoot at the target.";
        if (_hoveredUnit != null)
        {
            res += "\nACC:" + (int)_selfShotStats.GetAccuracy(_hoveredUnit, _effector.LinesOfSight[_hoveredUnit].cover) + // (_effector.Character.Accuracy - _hoveredUnit.Character.GetDodge(_effector.LinesOfSight[_hoveredUnit].cover)) +
                    "% | CRIT:" + (int)_selfShotStats.GetCritRate() + // + _effector.Character.CritChances +
                    "% | DMG:" + (int)_selfShotStats.GetDamage();// _effector.Character.Damage;
        }
        else if (_targetIndex >= 0)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];
            if (target == null) Debug.Log("BLIP BLOUP");

            res += "\nACC:" + (int)_selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover) + //(_effector.Character.Accuracy - target.Character.GetDodge(_effector.LinesOfSight[target].cover)) +
                    "% | CRIT:" + (int)_selfShotStats.GetCritRate() + // + _effector.Character.CritChances +
                    "% | DMG:" + (int)_selfShotStats.GetDamage();// _effector.Character.Damage;
        }

        return res;
    }

    public override void SetEffector(GridBasedUnit effector)
    {
        _possibleTargets = new List<GridBasedUnit>();

        foreach (GridBasedUnit unit in effector.LinesOfSight.Keys)
        {
            float distance = Vector2.Distance(unit.GridPosition, effector.GridPosition);
            if (distance <= effector.Character.RangeShot)
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

        base.SetEffector(effector);

        _selfShotStats = new AbilityStats(0, 0, 1f, 0, 0, _effector);
    }

    public override bool CanExecute()
    {
        return _targetIndex >= 0;
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

        int randShot = RandomEngine.Instance.Range(0, 100); // between 0 and 99
        int randCrit = RandomEngine.Instance.Range(0, 100);

        AbilityResult result = new AbilityResult();

        switch (_effector.AllyCharacter.CharacterClass)
        {
            case EnumClasses.Berserker:
                SoundManager.PlaySound(SoundManager.Sound.BasicPunch);
                break;
            case EnumClasses.Sniper:
                SoundManager.PlaySound(SoundManager.Sound.BasicShotSniper);
                break;
            default:
                SoundManager.PlaySound(SoundManager.Sound.BasicShotGatling);
                break;
        }
        if (randShot < _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover))
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, true);

            if (randCrit < _selfShotStats.GetCritRate())
            {
                result.Damage = AttackDamage(_effector, target as EnemyUnit, _effector.Character.Damage * 1.5f, true);
                result.Critical = true;
                SendResultToHistoryConsole(result);
            }
            else
            {
                result.Damage = AttackDamage(_effector, target as EnemyUnit, _effector.Character.Damage, false);
                result.Critical = false;
                SendResultToHistoryConsole(result);
            }
        }
        else
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, false);

            result.Miss = true;
            SendResultToHistoryConsole(result);
        }
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];

        if (result.Miss)
        {
            HistoryConsole.Instance
                .BeginEntry()
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" missed ").CloseTag()
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
                .AddText(" on ")
                .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
                .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(target.Character.Name).CloseTag()
                .CloseAllOpenedTags().Submit();
        }
        else
        {
            string criticalText = result.Critical ? " critical" : "";

            HistoryConsole.Instance
                .BeginEntry()
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
                .AddText(" used ")
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
                .AddText(" on ")
                .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
                .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(target.Character.Name).CloseTag()
                .AddText(": did ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage}{criticalText} damage").CloseTag()
                .CloseAllOpenedTags().Submit();
        }
    }

    protected override void EndAbility()
    {
        _targetIndex = -1;
        _possibleTargets = null;
        base.EndAbility();
    }


    public override void UISelectUnit(GridBasedUnit unit)
    {
        _targetIndex = _possibleTargets.IndexOf(unit);
        CombatGameManager.Instance.Camera.SwitchParenthood(unit);
        RequestDescriptionUpdate();
        RequestTargetSymbolUpdate(unit);
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
                if ((tile - user.GridPosition).magnitude <= user.Character.RangeShot)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
