using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Devouring : BaseDuoAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;

    public override string GetDescription()
    {
        string res = "You feed on an enemy, restoring your health triggering yout Bloodlust.";
        if (_chosenAlly != null && _hoveredUnit != null)
        {
            res += "\nACC:" + (int)_selfShotStats.GetAccuracy(_hoveredUnit, _effector.LinesOfSight[_hoveredUnit].cover) +
                    "% | CRIT:" + (int)_selfShotStats.GetCritRate() +
                    "% | DMG:" + (int)_selfShotStats.GetDamage() +
                    " | HEAL:" + (int)_selfShotStats.GetHeal() +
                    "\nBloodlust: CRIT+50%, ATK+50%," +
                    "\nDMG TKN+50%, 3TRN";
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nACC:" + (int)_selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover) +
                    "% | CRIT:" + (int)_selfShotStats.GetCritRate() +
                    "% | DMG:" + (int)_selfShotStats.GetDamage() +
                    " | HEAL:" + (int)_selfShotStats.GetHeal() +
                    "\nBloodlust: CRIT+50%, ATK+50%," +
                    "\nDMG TKN+50%, 3TRN";
        }
        else if (_effector != null & _temporaryChosenAlly != null)
        {
            var temporarySelfShotStat = new AbilityStats(999, 0, 1.5f, 0, 6, _effector);
            temporarySelfShotStat.UpdateWithEmotionModifiers(_effector);

            res += "\nACC:" + (int)temporarySelfShotStat.GetAccuracy() + "%" +
                    " | CRIT:" + (int)temporarySelfShotStat.GetCritRate() + "%" +
                    " | DMG:" + (int)temporarySelfShotStat.GetDamage() +
                    " | HEAL:" + (int)temporarySelfShotStat.GetHeal() +
                    "\nBloodlust: CRIT+50%, ATK+50%," +
                    "\nDMG TKN+50%, 3TRN";
        }
        return res;
    }

    public override string GetName()
    {
        return "Devouring";
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null && _targetIndex >= 0;
    }

    protected override void ChooseAlly()
    {
        _possibleTargets = new List<GridBasedUnit>();
        var tempTargets = new GridBasedUnit[_effector.LinesOfSight.Count];
        _effector.LinesOfSight.Keys.CopyTo(tempTargets, 0);

        foreach (GridBasedUnit unit in tempTargets)
        {
            if (    (_chosenAlly.LinesOfSight.ContainsKey(unit)) 
                &&  ((unit.GridPosition - this._effector.GridPosition).magnitude < 2))
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

        _selfShotStats = new AbilityStats(999, 0, 1.5f, 0, 6, _effector);

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
            RequestTargetSymbolUpdate(_possibleTargets[_targetIndex]);
        }
    }

    public override void Execute()
    {
        // Impact on the sentiments
        // Ally -> Self relationship
        AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, -10);

        // Actual effect of the ability
        GridBasedUnit target = _possibleTargets[_targetIndex];
        AbilityResult result = new AbilityResult();

        Debug.Log("DEVOURING : we are shooting at " + target.GridPosition + " with cover " + (int)_effector.LinesOfSight[target].cover);
        if (!StartAction(ActionTypes.Attack, _effector, _chosenAlly))
        {
            ShootResult shooResult = SelfShoot(target, _selfShotStats);
            float heal = _selfShotStats.GetHeal();
            _effector.Heal(ref heal);
            AddBuff(_effector, new Buff("Bloodlust", 6, _effector, damageBuff: 0.5f, critBuff: 0.5f, mitigationBuff: 1.5f));
            
            result.CopyShootResult(shooResult);
            result.Heal = heal;
        }
        else
        {
            result.Cancelled = true;
        }

        SoundManager.PlaySound(SoundManager.Sound.Devouring);
        SendResultToHistoryConsole(result);
    }

    // always hits and doesn't procc StartAction, because it was already tested above
    private ShootResult SelfShoot(GridBasedUnit target, AbilityStats selfShotStats)
    {
        int randCrit = RandomEngine.Instance.Range(0, 100);

        AttackHitOrMiss(_effector, target as EnemyUnit, true, _chosenAlly);

        if (randCrit < selfShotStats.GetCritRate())
        {
            float effectiveDamage = AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage() * 1.5f, true, _chosenAlly);
            return new ShootResult(false, true, effectiveDamage, true);
        }
        else
        {
            float effectiveDamage = AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage(), false, _chosenAlly);
            return new ShootResult(false, true, effectiveDamage, false);
        }
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];
        string healCriticalText = result.Critical ? " greatly" : "";
        string damageCriticalText = result.Critical ? " critical" : "";

        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
            .AddText(" performed ")
            .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .AddText(" in front of ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
            .AddText(":")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{healCriticalText} healed").CloseTag()
            .AddText(" themselves for ")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Heal} health points").CloseTag()
            .AddText(" and did ")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage}{damageCriticalText} damage").CloseTag()
            .AddText(" to ")
            .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
            .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(target.Character.Name).CloseTag()
            .Submit();
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude < 2;
    }

    public override string GetAllyDescription()
    {
        return "You hold the enemy while your ally devoures them, leaving you terrified.";
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
        return "A terrifying attack that heals and buffs.";
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
