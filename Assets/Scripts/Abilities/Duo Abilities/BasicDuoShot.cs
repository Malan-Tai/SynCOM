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

    public override void Execute()
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];
        Relationship relationshipAllyToSelf = _effector.AllyCharacter.Relationships[this._chosenAlly.AllyCharacter];
        SoundManager.PlaySound(SoundManager.Sound.BasicShotGatling);
        SoundManager.PlaySound(SoundManager.Sound.BasicShotSniper);

        //if (relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Sympathy) < 0 || relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Admiration) < 0 || relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Trust) < 0)
        //{
        //    SoundManager.PlaySound(SoundManager.Sound.RetentlessFoe);
        //}
        //else
        //{
        //    SoundManager.PlaySound(SoundManager.Sound.RetentlessNeutral);
        //}
        
        ShootResult selfResults = SelfShoot(target, _selfShotStats);
        ShootResult allyResults = AllyShoot(target, _allyShotStats);

        GetLine(selfResults, allyResults);

        AbilityResult result = new AbilityResult();
        result.CopyShootResult(selfResults);
        result.CopyAllyShootResult(allyResults);
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
            res += "\nACC:" + (int)_selfShotStats.GetAccuracy(_hoveredUnit, _effector.LinesOfSight[_hoveredUnit].cover) +
                    "% | CRIT:" + (int)_selfShotStats.GetCritRate() +
                    "% | DMG:" + (int)_selfShotStats.GetDamage();
        }
        else if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nACC:" + (int)_selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover) +
                    "% | CRIT:" + (int)_selfShotStats.GetCritRate() +
                    "% | DMG:" + (int)_selfShotStats.GetDamage();
        }
        else if (_effector != null & _temporaryChosenAlly != null)
        {
            var temporarySelfShotStat = new AbilityStats(0, 0, 1.5f, 0, 0, _effector);
            temporarySelfShotStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nACC:" + (int)temporarySelfShotStat.GetAccuracy() + "%" +
                    " | CRIT:" + (int)temporarySelfShotStat.GetCritRate() + "%" +
                    " | DMG:" + (int)temporarySelfShotStat.GetDamage();
        }

        return res;
    }

    public override string GetAllyDescription()
    {
        string res = "Take a shot at the target with augmented damage.";
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
        else if (_effector != null & _temporaryChosenAlly != null)
        {
            var temporaryAllyShotStat = new AbilityStats(0, 0, 1.5f, 0, 0, _temporaryChosenAlly);
            temporaryAllyShotStat.UpdateWithEmotionModifiers(_effector);

            res += "\nACC:" + (int)temporaryAllyShotStat.GetAccuracy() + "%" +
                    " | CRIT:" + (int)temporaryAllyShotStat.GetCritRate() + "%" +
                    " | DMG:" + (int)temporaryAllyShotStat.GetDamage();
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

    private void GetLine(ShootResult selfResults, ShootResult AllyResults)
    {
        if (!selfResults.Cancelled && selfResults.Landed)
        {
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Engineer)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultEngineerMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultEngineerMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultEngineerMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultEngineerMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultEngineerFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultEngineerFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultEngineerFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultEngineerFemale;
                        }
                    }
                }


            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Berserker)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultBerserkerMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultBerserkerMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultBerserkerMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultBerserkerMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultBerserkerFeMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultBerserkerFeMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultBerserkerFeMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultBerserkerFeMale;
                        }
                    }
                }


            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Alchemist)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultAlchemistMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultAlchemistMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultAlchemistMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultAlchemistMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultAlchemistFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultAlchemistFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultAlchemistFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultAlchemistFemale;
                        }
                    }
                }


            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Sniper)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultSniperMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultSniperMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultSniperMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultSniperMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultSniperFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultSniperFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultSniperFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultSniperFemale;
                        }
                    }
                }


            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Bodyguard)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultBodyguardMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultBodyguardMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultBodyguardMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultBodyguardMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultBodyguardFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultBodyguardFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultBodyguardFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultBodyguardFemale;
                        }
                    }
                }


            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Smuggler)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultSmugglerMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultSmugglerMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultSmugglerMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultSmugglerMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.EWRelentlessAssaultSmugglerFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FWRelentlessAssaultSmugglerFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FWRelentlessAssaultSmugglerFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.EWRelentlessAssaultSmugglerFemale;
                        }
                    }
                }


            }
        }
        if (!selfResults.Cancelled && !selfResults.Landed)
        {
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Engineer)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultEngineerMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultEngineerMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultEngineerMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultEngineerMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultEngineerFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultEngineerFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultEngineerFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultEngineerFemale;
                        }
                    }
                }
            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Berserker)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultBerserkerMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultBerserkerMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultBerserkerMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultBerserkerMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultBerserkerFeMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultBerserkerFeMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultBerserkerFeMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultBerserkerFeMale;
                        }
                    }
                }
            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Alchemist)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultAlchemistMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultAlchemistMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultAlchemistMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultAlchemistMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultAlchemistFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultAlchemistFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultAlchemistFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultAlchemistFemale;
                        }
                    }
                }
            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Sniper)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultSniperMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultSniperMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultSniperMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultSniperMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultSniperFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultSniperFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultSniperFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultSniperFemale;
                        }
                    }
                }
            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Bodyguard)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultBodyguardMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultBodyguardMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultBodyguardMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultBodyguardMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultBodyguardFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultBodyguardFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultBodyguardFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultBodyguardFemale;
                        }
                    }
                }
            }
            if (_effector.AllyCharacter.CharacterClass == EnumClasses.Smuggler)
            {
                if (_effector.AllyCharacter.Gender == EnumGender.Male || _effector.AllyCharacter.Gender == EnumGender.Other)
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultSmugglerMale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultSmugglerMale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultSmugglerMale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultSmugglerMale;
                        }
                    }
                }
                else
                {
                    if (GetRelationshipStatus(true) == -1)
                    {
                        _effectorSound = SoundManager.Sound.ELRelentlessAssaultSmugglerFemale;
                    }
                    else if (GetRelationshipStatus(true) == 1)
                    {
                        _effectorSound = SoundManager.Sound.FLRelentlessAssaultSmugglerFemale;
                    }
                    else
                    {
                        if (RandomEngine.Instance.Range(0, 2) == 0)
                        {
                            _effectorSound = SoundManager.Sound.FLRelentlessAssaultSmugglerFemale;
                        }
                        else
                        {
                            _effectorSound = SoundManager.Sound.ELRelentlessAssaultSmugglerFemale;
                        }
                    }
                }
            }
        }
    }
}
