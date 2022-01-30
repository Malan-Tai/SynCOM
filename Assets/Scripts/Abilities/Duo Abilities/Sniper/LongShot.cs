using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongShot : BaseDuoAbility
{
    private AbilityStats _selfShotStats;
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;
    private int _selectionRange = 20;
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
            res += "\nACC:" + (int)_selfShotStats.GetAccuracy(_hoveredUnit, _chosenAlly.LinesOfSight[_hoveredUnit].cover) +
                    "% | CRIT:" + (int)_selfShotStats.GetCritRate() +
                    "% | DMG:" + (int)_selfShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nACC:" + (int)_selfShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover) +
                    "% | CRIT:" + (int)_selfShotStats.GetCritRate() +
                    "% | DMG:" + (int)_selfShotStats.GetDamage();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporarySelfShotStat = new AbilityStats(0, 0, 2f, 0, 0, _effector);
            temporarySelfShotStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nACC:" + (int)temporarySelfShotStat.GetAccuracy() +
                    "% | CRIT:" + (int)temporarySelfShotStat.GetCritRate() +
                    "% | DMG:" + (int)temporarySelfShotStat.GetDamage();
        }

        return res;
    }

    public override string GetAllyDescription()
    {
        return "Indicate the position of an enemy to the Sniper, allowing them to shoot the enemy as if from your point of view.";
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - _effector.GridPosition).magnitude <= _selectionRange;
    }

    protected override void ChooseAlly()
    {
        _possibleTargets = new List<GridBasedUnit>();
        var tempTargets = new GridBasedUnit[_chosenAlly.LinesOfSight.Count];
        _chosenAlly.LinesOfSight.Keys.CopyTo(tempTargets, 0);

        foreach (GridBasedUnit unit in tempTargets)
        {
            float distanceToAlly = Vector2.Distance(unit.GridPosition, _chosenAlly.GridPosition);
            if (distanceToAlly <= _chosenAlly.Character.RangeShot)
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

        if (!BlockingUIElement.IsUIHovered && Physics.Raycast(ray, out hitData, 1000))
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
            //SoundManager.PlaySound(SoundManager.Sound.RetentlessFoe);
        }

        else
        {
            //SoundManager.PlaySound(SoundManager.Sound.RetentlessNeutral);
        }

        SoundManager.PlaySound(SoundManager.Sound.LongShot);
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
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_effector.Character.FirstName).CloseTag()
                .AddText(" cancelled ")
                .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
                .AddText(" with ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .AddText(" to do something else...");
        }
        else
        {
            HistoryConsole.Instance
                .BeginEntry()
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .AddText(" indicated the position of ")
                .OpenIconTag($"{_chosenAlly.LinesOfSight[target].cover}Cover").CloseTag()
                .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(target.Character.FirstName).CloseTag()
                .AddText(" so that ")
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_effector.Character.FirstName).CloseTag()
                .AddText(" can use ")
                .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag();

            if (result.Miss)
            {
                HistoryConsole.Instance
                    .AddText(": they ")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText("missed").CloseTag();
            }
            else
            {
                string criticalText = result.Critical ? " critical" : "";

                HistoryConsole.Instance
                    .AddText(": they did ")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage}{criticalText} damage").CloseTag();
            }
        }

        HistoryConsole.Instance.Submit();
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
                if ((tile - user.GridPosition).magnitude <= _selectionRange)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
