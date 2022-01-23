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
            res += "\nAcc: ~" + (int)_selfShotStats.GetAccuracy() + "%" +
                    " | Crit: ~" + (int)_selfShotStats.GetCritRate() + "%" +
                    " | Dmg: " + (int)_selfShotStats.GetDamage();
        }
        else if (_effector != null)
        {
            res += "\nAcc: ~" +  (int)_effector.AllyCharacter.Accuracy + "%" +
                    " | Crit: ~" + (int)_effector.AllyCharacter.CritChances + "%" +
                    " | Dmg: " + (int)_effector.AllyCharacter.Damage * 3;
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
                // La caméra se déplace bien, mais du coup la tile visée se déplace aussi. Voir le TODO plus haut.

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
                _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_tileCoord, 1);

                CombatGameManager.Instance.TileDisplay.DisplayTileZone("DamageZone", _areaOfEffectTiles, false);

                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    enemy.DontHighlightUnit();
                }

                _targets.Clear();
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    if (Mathf.Abs(enemy.GridPosition.x - _tileCoord.x) + Mathf.Abs(enemy.GridPosition.y - _tileCoord.y) <= 1)
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
        int randLaunch = UnityEngine.Random.Range(0, 100);

        var parametersLaunch = new InterruptionParameters { interruptionType = InterruptionType.FocusTargetUntilEndOfMovement, target = _effector, position = _tileCoord, pathfinding = PathfindingMoveType.Linear };
        _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parametersLaunch));


        if (randLaunch <= _launchingAccuracy)
        {
            // Launch successful : Damage enemies
            foreach (EnemyUnit target in _targets)
            {
                SelfShoot(target, _selfShotStats, alwaysHit: true, canCrit: false);
            }
        }
        else
        {
            // Launch failed : Damage dwarf
            SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Trust, -10);
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Admiration, -5);
            FriendlyFireDamage(_chosenAlly, _effector, _chosenAlly.AllyCharacter.Damage * 1f, _effector);
        }
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return //unit.AllyCharacter.Weigth >= 80 &&
               (unit.GridPosition - _effector.GridPosition).magnitude < 2;
    }

    protected override void EndAbility()
    {
        base.EndAbility();
        //CombatGameManager.Instance.TileDisplay.HideTileZone("DamageZone");
        //CombatGameManager.Instance.TileDisplay.HideTileZone("AttackZone");

        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            enemy.DontHighlightUnit();
        }
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        // TODO
    }
}
