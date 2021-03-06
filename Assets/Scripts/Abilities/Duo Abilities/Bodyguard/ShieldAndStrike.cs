using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldAndStrike : BaseDuoAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfProtStats;
    private AbilityStats _allyShotStats;

    public override string GetDescription()
    {
        string res = "You cover your ally while they attack, reducing damage received for the following turn.";
        if (_chosenAlly != null)
        {
            res += "\nPROT:" + (int)((1 - _selfProtStats.GetProtection()) * 100) + "%";
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporarySelfProtStat = new AbilityStats(0, 0, 0, 0.5f, 0, _effector);
            temporarySelfProtStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nPROT:" + (int)((1 - temporarySelfProtStat.GetProtection()) * 100) + "%";
        }
        else
        {
            res += "\nPROT:50%";
        }
        return res;
    }

    public override string GetName()
    {
        return "Shield & Strike";
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
            if ((_chosenAlly.LinesOfSight.ContainsKey(unit))
                && ((unit.GridPosition - _chosenAlly.GridPosition).magnitude <= _chosenAlly.Character.RangeShot))
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

        _selfProtStats = new AbilityStats(0, 0, 0, 0.5f, 0, _effector);
        _allyShotStats = new AbilityStats(0, 0, 1.5f, 0, 0, _chosenAlly);

        _selfProtStats.UpdateWithEmotionModifiers(_chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);
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
            RequestTargetSymbolUpdate(_possibleTargets[_targetIndex]);
        }
    }

    public override void Execute()
    {
        // Actual effect of the ability
        GridBasedUnit target = _possibleTargets[_targetIndex];

        AbilityResult result = new AbilityResult();
        ShootResult shootResult = AllyShoot(target, _allyShotStats);
        result.CopyAllyShootResult(shootResult);

        if (!StartAction(ActionTypes.Protect, _effector, _chosenAlly))
        {
            AddBuff(_chosenAlly, new ProtectedByBuff(2, _chosenAlly, _effector, _selfProtStats.GetProtection()));

            // Impact on the sentiments
            // Ally -> Self relationship
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, 5);
            SoundManager.PlaySound(SoundManager.Sound.ShieldAndStrike);
        }
        else
        {
            result.Cancelled = true;
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, -5);
            Debug.Log("refused to protecc");
        }

        SendResultToHistoryConsole(result);
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];

        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
            .AddText(_effector.Character.FirstName).CloseTag()
            .AddText(" used ")
            .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .AddText(" with ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
            .AddText(_chosenAlly.Character.FirstName).CloseTag();

        if (result.Cancelled)
        {
            HistoryConsole.Instance
                .AddText(" but")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" refused to protect").CloseTag()
                .AddText(" them. ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .AddText(" still shot and ");
        }
        else
        {
            HistoryConsole.Instance
                .AddText(" to protect them for ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{(int)((1 - _selfProtStats.GetProtection()) * 100)}%")
                .AddText(" while ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .AddText(" shot and ");
        }

        if (result.AllyMiss)
        {
            HistoryConsole.Instance
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText("missed").CloseTag()
                .AddText(" their shot on ")
                .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
                .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(target.Character.FirstName).CloseTag();
        }
        else
        {
            string criticalText = result.AllyCritical ? " critical" : "";

            HistoryConsole.Instance
                .AddText("did ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.AllyDamage}{criticalText} damage").CloseTag()
                .AddText(" to ")
                .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
                .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(target.Character.FirstName).CloseTag();
        }

        HistoryConsole.Instance.Submit();
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude < 2;
    }

    public override string GetAllyDescription()
    {
        string res = "Thanks to your ally's protection, you can focus solely on your shot.";

        if (_chosenAlly != null && _hoveredUnit != null)
        {
            res += "\nACC:" + (int)_allyShotStats.GetAccuracy(_hoveredUnit, _chosenAlly.LinesOfSight[_hoveredUnit].cover) +
                    "% | CRIT:" + (int)_allyShotStats.GetCritRate() +
                    "% | DMG:" + (int)_allyShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nACC:" + (int)_allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover) +
                    "% | CRIT:" + (int)_allyShotStats.GetCritRate() +
                    "% | DMG:" + (int)_allyShotStats.GetDamage();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporaryAllyShotStat = new AbilityStats(0, 0, 1.5f, 0, 0, _temporaryChosenAlly);
            temporaryAllyShotStat.UpdateWithEmotionModifiers(_effector);

            res += "\nACC:" + (int)temporaryAllyShotStat.GetAccuracy() +
                    "% | CRIT:" + (int)temporaryAllyShotStat.GetCritRate() +
                    "% | DMG:" + (int)temporaryAllyShotStat.GetDamage();
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
        return "Protects an ally while they shoot a single target.";
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
