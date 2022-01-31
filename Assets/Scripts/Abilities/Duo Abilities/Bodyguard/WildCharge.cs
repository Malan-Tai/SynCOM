using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildCharge : BaseDuoAbility
{
    private int _radius = 6;
    private AbilityStats _selfProtStats;
    private AbilityStats _allyShotStats;

    private LayerMask _groundLayerMask = LayerMask.GetMask("Ground");

    private List<EnemyUnit> _targets = new List<EnemyUnit>();

    private Vector2Int _previousTileCoord;
    private Vector2Int _tileCoord;

    private List<Tile> _areaOfEffectTiles = new List<Tile>();
    private List<Tile> _possibleTargetsTiles = new List<Tile>();

    public override string GetName()
    {
        return "Wild Charge";
    }

    public override string GetShortDescription()
    {
        return "Charge in a direction and defend yourself the next turn.";
    }

    public override string GetDescription()
    {
        string res = "You both charge in a direction : you hold a shield, defending both of you for the following turn.";
        if (_chosenAlly != null)
        {
            res += "\nPROT: " + (int)((1 - _selfProtStats.GetProtection()) * 100) + "%";
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporarySelfProtStat = new AbilityStats(0, 0, 0, 0.3f, 0, _effector);
            temporarySelfProtStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nPROT: " + (int)((1 - temporarySelfProtStat.GetProtection()) * 100) + "%";
        }
        else
        {
            res += "\nPROT:30%";
        }
        return res;
    }

    public override string GetAllyDescription()
    {
        string res = "Thanks to your ally's protection, you can focus solely on your shots.";

        if (_chosenAlly != null)
        {
            res += "\nACC:" + (int)_allyShotStats.GetAccuracy() +
                    "% | CRIT:" + (int)_allyShotStats.GetCritRate() +
                    "% | DMG:" + (int)_allyShotStats.GetDamage();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporaryAllyShotStat = new AbilityStats(0, 0, 1.5f, 0, 0, _temporaryChosenAlly);
            temporaryAllyShotStat.UpdateWithEmotionModifiers(_effector);

            res += "\nACC:100" + // + (int)temporaryAllyShotStat.GetAccuracy() +
                    "% | CRIT:" + (int)temporaryAllyShotStat.GetCritRate() +
                    "% | DMG:" + (int)temporaryAllyShotStat.GetDamage();
        }

        return res;
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - _effector.GridPosition).magnitude < 2;
    }

    protected override void ChooseAlly()
    {
        _selfProtStats = new AbilityStats(0, 0, 0, 0.7f, 0, _effector);
        _selfProtStats.UpdateWithEmotionModifiers(_chosenAlly);

        _allyShotStats = new AbilityStats(0, 0, 1.5f, 0, 0, _chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);

        _possibleTargetsTiles.Clear();
        GridMap map = CombatGameManager.Instance.GridMap;
        int x = _effector.GridPosition.x;
        int y = _effector.GridPosition.y;
        // TODO: be stopped by walls !
        //for (int n = -_radius; n <= _radius; n++)
        //{
        //    if (map[x + n, y].IsWalkable) _possibleTargetsTiles.Add(map[x + n, y]);
        //    if (map[x, y + n].IsWalkable) _possibleTargetsTiles.Add(map[x, y + n]);
        //}

        for (int n = 1; n <= _radius; n++)
        {
            if (map[x + n, y].IsWalkable && !map.OccupiedTiles.Contains(new Vector2Int(x + n, y))) _possibleTargetsTiles.Add(map[x + n, y]);
            else break;
        }
        for (int n = 1; n <= _radius; n++)
        {
            if (map[x - n, y].IsWalkable && !map.OccupiedTiles.Contains(new Vector2Int(x - n, y))) _possibleTargetsTiles.Add(map[x - n, y]);
            else break;
        }
        for (int n = 1; n <= _radius; n++)
        {
            if (map[x, y + n].IsWalkable && !map.OccupiedTiles.Contains(new Vector2Int(x, y + n))) _possibleTargetsTiles.Add(map[x, y + n]);
            else break;
        }
        for (int n = 1; n <= _radius; n++)
        {
            if (map[x, y - n].IsWalkable && !map.OccupiedTiles.Contains(new Vector2Int(x, y - n))) _possibleTargetsTiles.Add(map[x, y - n]);
            else break;
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", _possibleTargetsTiles, false);
    }

    protected override void EnemyTargetingInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (!BlockingUIElement.IsUIHovered && Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
        {
            // J'affiche la zone cibl�e, en mettant � jour les tiles (ce sont celles situ�es � port�e de la tile cibl�e)

            var temporaryTileCoord = CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point);
            if (!_possibleTargetsTiles.Contains(CombatGameManager.Instance.GridMap[temporaryTileCoord]))
            {
                return;
            }
            else
            {
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

                _areaOfEffectTiles.Clear();
                _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectCorridor(_effector.GridPosition, _tileCoord, 1);

                CombatGameManager.Instance.TileDisplay.DisplayTileZone("DamageZone", _areaOfEffectTiles, false);

                // Je cache le highlight des anciennes targets
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    enemy.DontHighlightUnit();
                }
                foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
                {
                    ally.DontHighlightUnit();
                }

                // Pour mettre les cibles en surbrillance
                _targets.Clear();
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    if (_areaOfEffectTiles.Contains(CombatGameManager.Instance.GridMap[enemy.GridPosition]))
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
        var parametersLaunchSelf = new InterruptionParameters { interruptionType = InterruptionType.FocusTargetUntilEndOfMovement, target = _effector, position = _tileCoord, pathfinding = PathfindingMoveType.Linear };
        _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parametersLaunchSelf));
        AbilityResult result = new AbilityResult();

        // Calculate arrival coordinates of ally
        Vector2Int allyArrival = _tileCoord;
        if (_tileCoord.x == _effector.GridPosition.x)
        {
            allyArrival.x = _tileCoord.x;
            if (_tileCoord.y <= _effector.GridPosition.y) allyArrival.y = _tileCoord.y + 1;
            else allyArrival.y = _tileCoord.y - 1;
        }
        else if (_tileCoord.y == _effector.GridPosition.y)
        {
            allyArrival.y = _tileCoord.y;
            if (_tileCoord.x <= _effector.GridPosition.x) allyArrival.x = _tileCoord.x + 1;
            else allyArrival.x = _tileCoord.x - 1;
        }

        var parametersLaunchAlly = new InterruptionParameters { interruptionType = InterruptionType.FocusTargetUntilEndOfMovement, target = _chosenAlly, position = allyArrival, pathfinding = PathfindingMoveType.Linear };
        _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parametersLaunchAlly));

        if (!StartAction(ActionTypes.Protect, _effector, _chosenAlly))
        {
            AddBuff(_chosenAlly, new ProtectedByBuff(2, _chosenAlly, _effector, _selfProtStats.GetProtection()));

            // Impact on the sentiments
            // Ally -> Self relationship
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, 5);
        }
        else
        {
            result.Cancelled = true;
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, -5);
            Debug.Log("refused to protecc");
        }

        // Damage enemies
        SoundManager.PlaySound(SoundManager.Sound.WildCharge);
        foreach (EnemyUnit target in _targets)
        {
            if (RandomEngine.Instance.Range(0, 100) < _allyShotStats.GetCritRate())
                result.DamageList.Add(AttackDamage(_chosenAlly, target, _allyShotStats.GetDamage() * 1.5f, true, _effector));
            else
                result.DamageList.Add(AttackDamage(_chosenAlly, target, _allyShotStats.GetDamage(), false, _effector));
        }

        AttackHitOrMiss(_chosenAlly, null, _targets.Count > 0, _effector);

        SendResultToHistoryConsole(result);
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
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
                .AddText(" still attacked, dealing ");
        }
        else
        {
            HistoryConsole.Instance
                .AddText(" to protect them for ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{(int)((1 - _selfProtStats.GetProtection()) * 100)}%").CloseTag()
                .AddText(" while ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_chosenAlly.Character.FirstName).CloseTag()
                .AddText(" dealt ");
        }

        if (_targets.Count == 0)
        {
            HistoryConsole.Instance.AddText("damage to no one");
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

        HistoryConsole.Instance.Submit();
    }

    protected override void EndAbility()
    {
        base.EndAbility();

        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            enemy.DontHighlightUnit();
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
                if ((tile - user.GridPosition).magnitude < 2)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
