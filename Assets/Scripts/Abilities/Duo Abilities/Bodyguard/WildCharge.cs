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
            res += "\nPROT: " + (1 - _selfProtStats.GetProtection()) * 100 + "%";
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporarySelfProtStat = new AbilityStats(0, 0, 0, 0.3f, 0, _effector);
            temporarySelfProtStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nPROT: " + (1 - temporarySelfProtStat.GetProtection()) * 100 + "%";
        }
        return res;
    }

    public override string GetAllyDescription()
    {
        string res = "Thanks to your ally's protection, you can focus solely on your shot.";

        if (_chosenAlly != null)
        {
            res += "\nAcc:~" + _allyShotStats.GetAccuracy() +
                    "% | Crit:" + _allyShotStats.GetCritRate() +
                    "% | Dmg:" + _allyShotStats.GetDamage();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporaryAllyShotStat = new AbilityStats(0, 0, 1, 0, 0, _temporaryChosenAlly);
            temporaryAllyShotStat.UpdateWithEmotionModifiers(_effector);

            res += "\nAcc:~" + temporaryAllyShotStat.GetAccuracy() +
                    "% | Crit:" + temporaryAllyShotStat.GetCritRate() +
                    "% | Dmg:" + temporaryAllyShotStat.GetDamage();
        }

        return res;
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - _effector.GridPosition).magnitude < 2;
    }

    protected override void ChooseAlly()
    {
        _selfProtStats = new AbilityStats(0, 0, 0, 0.3f, 0, _effector);
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
            if (map[x + n, y].IsWalkable) _possibleTargetsTiles.Add(map[x + n, y]);
            else break;
        }
        for (int n = 1; n <= _radius; n++)
        {
            if (map[x - n, y].IsWalkable) _possibleTargetsTiles.Add(map[x - n, y]);
            else break;
        }
        for (int n = 1; n <= _radius; n++)
        {
            if (map[x, y + n].IsWalkable) _possibleTargetsTiles.Add(map[x, y + n]);
            else break;
        }
        for (int n = 1; n <= _radius; n++)
        {
            if (map[x, y - n].IsWalkable) _possibleTargetsTiles.Add(map[x, y - n]);
            else break;
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", _possibleTargetsTiles, false);
    }

    protected override void EnemyTargetingInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
        {
            // J'affiche la zone ciblée, en mettant à jour les tiles (ce sont celles situées à portée de la tile ciblée)

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

        // Damage enemies
        SoundManager.PlaySound(SoundManager.Sound.WildCharge);
        foreach (EnemyUnit target in _targets)
        {
            SelfShoot(target, _allyShotStats, alwaysHit: true, canCrit: false);
        }
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        // TODO
    }

    protected override void EndAbility()
    {
        base.EndAbility();

        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            enemy.DontHighlightUnit();
        }
    }
}
