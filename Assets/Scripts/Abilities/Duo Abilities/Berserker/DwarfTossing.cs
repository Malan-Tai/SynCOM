using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DwarfTossing : BaseDuoAbility
{
    private AbilityStats _selfShotStats;

    private LayerMask _groundLayerMask = LayerMask.GetMask("Ground");

    private List<EnemyUnit> _targets = new List<EnemyUnit>();

    private Vector2Int _previousTileCoord;
    private Vector2Int _tileCoord;

    private List<Tile> _areaOfEffectTiles = new List<Tile>();
    private List<Tile> _possibleTargetsTiles = new List<Tile>();

    private int _throwingRadius = 10; // Depends on weight of launcher (and dwarf ?)
    private float _launchingAccuracy;

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    public override string GetAllyDescription()
    {
        string res = "You launch your ally in the air. If you fail your throw, they will take damage.";
        if (_chosenAlly != null)
        {
            res += "\nAcc: " + (int)_launchingAccuracy + "%" +
                   "\nDMG if fail: " + (int)_chosenAlly.AllyCharacter.Damage * 0.5;
        }
        else if (_effector != null && _temporaryChosenAlly != null)
        {
            res += "\nDMG if fail: " + (int)_temporaryChosenAlly.AllyCharacter.Damage * 0.5;
        }
        return res;
    }

    public override string GetDescription()
    {
        string res = "Your ally launches you in the air to land a devastating blow in the ennemy.";
        if (_chosenAlly != null)
        {
            res += "\nACC:" + (int)_selfShotStats.GetAccuracy() + "%" +
                    " | CRIT:" + (int)_selfShotStats.GetCritRate() + "%" +
                    " | DMG:" + (int)_selfShotStats.GetDamage();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporarySelfShotStat = new AbilityStats(0, 0, 3f, 0, 0, _effector);
            temporarySelfShotStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nACC:" + (int)temporarySelfShotStat.GetAccuracy() + "%" +
                    " | CRIT:" + (int)temporarySelfShotStat.GetCritRate() + "%" +
                    " | DMG:" + (int)temporarySelfShotStat.GetDamage();
        }
        else if (_effector != null)
        {
            res += "\nACC:" +  (int)_effector.AllyCharacter.Accuracy + "%" +
                    " | CRIT:" + (int)_effector.AllyCharacter.CritChances + "%" +
                    " | DMG:" + (int)_effector.AllyCharacter.Damage * 3;
        }
        return res;
    }

    public override string GetName()
    {
        return "Dwarf Tossing";
    }

    public override string GetShortDescription()
    {
        return "Get launched by an ally.";
    }

    protected override void ChooseAlly()
    {
        _selfShotStats = new AbilityStats(0, 0, 3f, 0, 0, _effector);
        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);

        // Update _throwingRadius depending in weight of launcher and dwarf
        // base radius = 8 at 100, 12 at 150, 4 at 50
        // bonus depending on dwarf weight : +1 if <90, -1 of > 90
        _throwingRadius = (int)(8 * _chosenAlly.AllyCharacter.Weigth / 100);
        if (_effector.AllyCharacter.Weigth < 90) _throwingRadius += 1;
        else if (_effector.AllyCharacter.Weigth > 90) _throwingRadius -= 1;

        _possibleTargetsTiles.Clear();
        GridMap map = CombatGameManager.Instance.GridMap;
        for (int i = 0; i < map.GridTileWidth; i++)
        {
            for (int j = 0; j < map.GridTileHeight; j++)
            {
                Vector2Int tile = new Vector2Int(i, j);
                if (map[tile].IsWalkable &&
                    !map.OccupiedTiles.Contains(tile) &&
                    (tile - _effector.GridPosition).magnitude <= _throwingRadius &&
                    (tile - _chosenAlly.GridPosition).magnitude <= _chosenAlly.AllyCharacter.RangeShot)
                {
                    _possibleTargetsTiles.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", _possibleTargetsTiles, false);
    }

    protected override void EnemyTargetingInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (!BlockingUIElement.IsUIHovered && Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
        {
            // J'affiche la zone cibl?e, en mettant ? jour les tiles (ce sont celles situ?es ? port?e de la tile cibl?e)

            var temporaryTileCoord = CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point);
            if (!_possibleTargetsTiles.Contains(CombatGameManager.Instance.GridMap[temporaryTileCoord]))
            {
                return;
            }
            else
            {
                // La cam?ra se d?place bien, mais du coup la tile vis?e se d?place aussi. Voir le TODO plus haut.

                bool clicked = Input.GetMouseButtonUp(0);
                if (clicked)
                {
                    UIConfirm();
                }

                CombatGameManager.Instance.TileDisplay.DisplayMouseHoverTileAt(temporaryTileCoord);

                if (temporaryTileCoord == _previousTileCoord)
                {
                    return;
                }
                _previousTileCoord = temporaryTileCoord;
                _tileCoord = temporaryTileCoord;

                // 50% at _throwingRadius 
                _launchingAccuracy = Mathf.Clamp(100 * (1 - (_chosenAlly.GridPosition - _tileCoord).magnitude / (2 * _throwingRadius)), 0, 100);
                RequestDescriptionUpdate();

                _areaOfEffectTiles.Clear();
                _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_tileCoord, 2);

                CombatGameManager.Instance.TileDisplay.DisplayTileZone("DamageZone", _areaOfEffectTiles, false);

                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    enemy.DontHighlightUnit();
                }

                _targets.Clear();
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    if (Mathf.Abs(enemy.GridPosition.x - _tileCoord.x) + Mathf.Abs(enemy.GridPosition.y - _tileCoord.y) <= 2)
                    {
                        _targets.Add(enemy);
                        enemy.HighlightUnit(Color.red);
                    }
                }
            }
        }
    }

    public override void Execute()
    {
        SoundManager.PlaySound(SoundManager.Sound.DwarfToss);

        int randLaunch = RandomEngine.Instance.Range(0, 100);
        AbilityResult result = new AbilityResult();

        var parametersLaunch = new InterruptionParameters { interruptionType = InterruptionType.FocusTargetUntilEndOfMovement, target = _effector, position = _tileCoord, pathfinding = PathfindingMoveType.Linear };
        _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parametersLaunch));

        if (randLaunch <= _launchingAccuracy)
        {
            result.AllyMiss = false;
            AttackHitOrMiss(_chosenAlly, null, true, _effector);

            // Launch successful : Damage enemies
            foreach (EnemyUnit target in _targets)
            {
                if (RandomEngine.Instance.Range(0, 100) < _selfShotStats.GetCritRate())
                    result.DamageList.Add(AttackDamage(_effector, target, _selfShotStats.GetDamage() * 1.5f, true, _chosenAlly));
                else
                    result.DamageList.Add(AttackDamage(_effector, target, _selfShotStats.GetDamage(), false, _chosenAlly));
            }
        }
        else
        {
            result.AllyMiss = true;

            // Launch failed : Damage dwarf
            result.AllyDamage = FriendlyFireDamage(_chosenAlly, _effector, _chosenAlly.AllyCharacter.Damage * 1f, _effector);
        }

        SendResultToHistoryConsole(result);
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return //unit.AllyCharacter.Weigth >= 80 &&
               (unit.GridPosition - _effector.GridPosition).magnitude < 2;
    }

    protected override void EndAbility()
    {
        base.EndAbility();

        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            enemy.DontHighlightUnit();
        }
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
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .AddText(": ");

        if (result.AllyMiss)
        {
            HistoryConsole.Instance
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.FirstName).CloseTag()
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(" missed").CloseTag()
                .AddText(", damaging ")
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_effector.Character.FirstName).CloseTag()
                .AddText(" for ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(result.AllyDamage.ToString()).CloseTag();
        }
        else
        {
            if (_targets.Count == 0)
            {
                HistoryConsole.Instance.AddText("damaged no one");
            }
            else
            {
                HistoryConsole.Instance.AddText("did ");
            }

            for (int i = 0; i < _targets.Count; i++)
            {
                if (i != 0)
                {
                    if (i == _targets.Count - 1)
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
                    .OpenLinkTag(_targets[i].Character.Name, _targets[i], EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                    .AddText(_targets[i].Character.FirstName).CloseTag();
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
                if ((tile - user.GridPosition).magnitude < 2)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
