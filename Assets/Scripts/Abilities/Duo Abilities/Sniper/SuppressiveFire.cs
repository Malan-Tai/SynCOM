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
            res += "\nACC:" + (int)_allyShotStats.GetAccuracy(_hoveredUnit, _chosenAlly.LinesOfSight[_hoveredUnit].cover) + "%" + 
                    " | CRIT: 100%" +
                    " | DMG:" + (int)_allyShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nACC:" + (int)_allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover) + "%" +
                    " | CRIT: 100%" +
                    " | DMG:" + (int)_allyShotStats.GetDamage();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporaryAllyShotStat = new AbilityStats(0, 9999, 2f, 0, 0, _temporaryChosenAlly);
            temporaryAllyShotStat.UpdateWithEmotionModifiers(_effector);

            res += "\nACC:" + (int)temporaryAllyShotStat.GetAccuracy() + "%" +
                    " | CRIT: 100%" +
                    " | DMG:" + (int)temporaryAllyShotStat.GetDamage();
        }

        return res;
    }

    public override string GetDescription()
    {
        string res = "Shoot at a distant enemy to distract them.";
        if (_chosenAlly != null && _hoveredUnit != null)
        {
            res += "\nACC:" + SniperAccuracy(_hoveredUnit) + "%" +
                    " | CRIT:" + _selfShotStats.GetCritRate() + "%" +
                    " | DMG:" + _selfShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nACC:" + SniperAccuracy(target) + "%" +
                    " | CRIT:" + _selfShotStats.GetCritRate() + "%" +
                    " | DMG:" + _selfShotStats.GetDamage();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporarySelfShotStat = new AbilityStats(0, 0, 1f, 0, 0, _effector);
            temporarySelfShotStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nCRIT:" + temporarySelfShotStat.GetCritRate() + "%" +
                   " | DMG:" + temporarySelfShotStat.GetDamage();
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
            if (distanceToSelf > _effector.Character.RangeShot/2 && distanceToAlly <= _chosenAlly.Character.RangeShot && _chosenAlly.LinesOfSight.ContainsKey(unit))
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
        AbilityResult result = new AbilityResult();
        SoundManager.PlaySound(SoundManager.Sound.SuppressiveFire);
        ShootResult selfResults = SelfShoot(target, _selfShotStats);
        ShootResult allyResult = AllyShoot(target, _allyShotStats);

        result.CopyShootResult(selfResults);
        result.CopyAllyShootResult(allyResult);

        if (!selfResults.Cancelled && selfResults.Landed)
        {
            if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
            {
                if (GetRelationshipStatus(true) == -1)
                {
                    _effectorSound = SoundManager.Sound.EWSuppressiveFireMale;
                }
                else if (GetRelationshipStatus(true) == 1)
                {
                    _effectorSound = SoundManager.Sound.FWSuppressiveFireMale;
                }
                else
                {
                    if (RandomEngine.Instance.Range(0, 2) == 0)
                    {
                        _effectorSound = SoundManager.Sound.FWSuppressiveFireMale;
                    }
                    else
                    {
                        _effectorSound = SoundManager.Sound.EWSuppressiveFireMale;
                    }
                }
            }
            else
            {
                if (GetRelationshipStatus(true) == -1)
                {
                    _effectorSound = SoundManager.Sound.EWSuppressiveFireFemale;
                }
                else if (GetRelationshipStatus(true) == 1)
                {
                    _effectorSound = SoundManager.Sound.FWSuppressiveFireFemale;
                }
                else
                {
                    if (RandomEngine.Instance.Range(0, 2) == 0)
                    {
                        _effectorSound = SoundManager.Sound.FWSuppressiveFireFemale;
                    }
                    else
                    {
                        _effectorSound = SoundManager.Sound.EWSuppressiveFireFemale;
                    }
                }
            }
        }

        if (!selfResults.Cancelled && !selfResults.Landed)
        {
            if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
            {
                if (GetRelationshipStatus(true) == -1)
                {
                    _effectorSound = SoundManager.Sound.ELSuppressiveFireMale;
                }
                else if (GetRelationshipStatus(true) == 1)
                {
                    _effectorSound = SoundManager.Sound.FLSuppressiveFireMale;
                }
                else
                {
                    if (RandomEngine.Instance.Range(0, 2) == 0)
                    {
                        _effectorSound = SoundManager.Sound.FLSuppressiveFireMale;
                    }
                    else
                    {
                        _effectorSound = SoundManager.Sound.ELSuppressiveFireMale;
                    }
                }
            }
            else
            {
                if (GetRelationshipStatus(true) == -1)
                {
                    _effectorSound = SoundManager.Sound.ELSuppressiveFireFemale;
                }
                else if (GetRelationshipStatus(true) == 1)
                {
                    _effectorSound = SoundManager.Sound.FLSuppressiveFireFemale;
                }
                else
                {
                    if (RandomEngine.Instance.Range(0, 2) == 0)
                    {
                        _effectorSound = SoundManager.Sound.FLSuppressiveFireFemale;
                    }
                    else
                    {
                        _effectorSound = SoundManager.Sound.ELSuppressiveFireFemale;
                    }
                }
            }
        }

        SendResultToHistoryConsole(result);
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];

        string selfCriticalText = result.Critical ? " critical" : "";
        string allyCriticalText = result.AllyCritical ? " critical" : "";

        if (result.Cancelled)
        {
            HistoryConsole.Instance
                .BeginEntry()
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_effector.Character.FirstName).CloseTag()
                .AddText(" started to use ")
                .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
                .AddText(" with ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .AddText(" but changed their mind and cancelled their action... ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .AddText(" still shot ")
                .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
                .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(target.Character.FirstName).CloseTag();

            if (result.Miss)
            {
                HistoryConsole.Instance
                    .AddText(" but")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" missed ").CloseTag();
            }
            else
            {
                HistoryConsole.Instance
                    .AddText(", dealing ")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage}{selfCriticalText} damage").CloseTag();
            }
        }
        else if (result.AllyCancelled)
        {
            HistoryConsole.Instance
                .BeginEntry()
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_effector.Character.FirstName).CloseTag()
                .AddText(" tried to use ")
                .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
                .AddText(" with ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .AddText(", ")
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_effector.Character.FirstName).CloseTag();

            if (result.Miss)
            {
                HistoryConsole.Instance
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" missed ").CloseTag()
                    .AddText(" their shot on ");
            }
            else
            {
                HistoryConsole.Instance
                    .AddText(" effectively did ")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage}{selfCriticalText} damage").CloseTag()
                    .AddText(" to ");
            }

            HistoryConsole.Instance
                .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
                .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(target.Character.FirstName).CloseTag()
                .AddText(" but ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" cancelled ").CloseTag()
                .AddText(" their shot to do something else...");
        }
        else
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
               .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
               .AddText(" on ")
               .OpenIconTag($"{_effector.LinesOfSight[target].cover}Cover").CloseTag()
               .OpenLinkTag(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
               .AddText(target.Character.FirstName).CloseTag()
               .AddText(": ")
               .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
               .AddText(_effector.Character.FirstName).CloseTag();

            if (result.Miss)
            {
                HistoryConsole.Instance.OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" missed ").CloseTag();
            }
            else
            {
                HistoryConsole.Instance
                    .AddText(" did ")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage}{selfCriticalText} damage").CloseTag();
            }

            HistoryConsole.Instance
                .AddText(" and ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag();

            if (result.AllyMiss)
            {
                HistoryConsole.Instance.OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" missed").CloseTag();
            }
            else
            {
                HistoryConsole.Instance
                    .AddText(" did ")
                    .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.AllyDamage}{allyCriticalText} damage").CloseTag();
            }
        }

        HistoryConsole.Instance.Submit();
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

        //float distPenalty = Mathf.Clamp01(dist / 10);

        //acc = acc * (1 - distPenalty);

        if (dist > 0) acc -= dist * 10;

        return acc;
    }

    protected override ShootResult SelfShoot(GridBasedUnit target, AbilityStats selfShotStats, bool alwaysHit = false, bool canCrit = true)
    {
        if (StartAction(ActionTypes.Attack, _effector, _chosenAlly))
        {
            return new ShootResult(true, false, 0f, false);
        }

        int randShot = RandomEngine.Instance.Range(0, 100); // between 0 and 99
        int randCrit = RandomEngine.Instance.Range(0, 100);

        if (alwaysHit || randShot < SniperAccuracy(target))
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

    public override void ShowRanges(AllyUnit user)
    {
        GridMap map = CombatGameManager.Instance.GridMap;
        List<Tile> range = new List<Tile>();

        for (int i = 0; i < map.GridTileWidth; i++)
        {
            for (int j = 0; j < map.GridTileHeight; j++)
            {
                Vector2Int tile = new Vector2Int(i, j);
                if ((tile - user.GridPosition).magnitude > user.Character.RangeShot/2)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
