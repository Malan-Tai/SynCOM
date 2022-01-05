using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeTossEngineer : BaseDuoAbility
{
    private LayerMask _groundLayerMask = LayerMask.GetMask("Ground");
    private TileDisplay _targetedTiles;
    private TileDisplay _targetableTiles;
    List<Tile> _areaOfEffectTiles = new List<Tile>();
    private Vector2Int _previousTileCoord;
    List<EnemyUnit> targets = new List<EnemyUnit>();
    List<AllyUnit> allyTargets = new List<AllyUnit>();
    Vector2Int tileCoord;

    List<Tile> _possibleTargetsTiles = new List<Tile>();

    // Utilisé pour déterminer l'accuracy de l'allié
    private AbilityStats _allyShotStats;
    // Utilisé pour déterminer les dégâts infligés
    private AbilityStats _selfShotStats;

    private int _radius = 3;

    public GrenadeTossEngineer()
    {
        GameObject map = GameObject.Find("Map");
        if (map == null)
        {
            Debug.Log("GameObject [Map] not found");
        }
        else
        {
            _targetedTiles = map.transform.Find("TileDisplay").GetComponent<TileDisplay>();
            _targetableTiles = map.transform.Find("TileDisplay").GetComponent<TileDisplay>();
        }
    }

    public override string GetAllyDescription()
    {
        return "You shoot the grenade midair with you expert precision. If you succeed, " +
                "the grenade benefits from an increased explosion radius.";
    }

    public override string GetDescription()
    {
        string res = "You throw a grenade in the air for the Sniper to shoot at";
        if (_chosenAlly != null)
        {
            res += "\nAcc: 100%" +
                    " | Crit: 0%" +
                    " | Dmg: " + _selfShotStats.GetDamage();
        }
        else if (_chosenAlly == null)
        {
            res += "\nAcc: 100%" +
                    " | Crit: 0%" +
                    " | Dmg: " + _effector.AllyCharacter.Damage * 1.5;
        }
        return res;
    }

    public override string GetName()
    {
        return "Grenade Toss";
    }

    public override bool CanExecute()
    {
        // position de la grenade :
        // - à 5 tiles de l'effector
        // - à porté de l'ally
        return _chosenAlly != null
            && (tileCoord - _effector.GridPosition).magnitude <= 5
            && (tileCoord - _chosenAlly.GridPosition).magnitude <= _chosenAlly.AllyCharacter.RangeShot;
    }

    protected override void ChooseAlly()
    {
        _allyShotStats = new AbilityStats(0, 0, 0, 0, _chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);

        _selfShotStats = new AbilityStats(0, 0, 1.5f, 0, _effector);
        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);

        // TODO: afficher les tiles qu'on peut cibler
        _possibleTargetsTiles.Clear();
        GridMap map = CombatGameManager.Instance.GridMap;
        for (int i = 0; i < map.GridTileWidth; i++)
        {
            for (int j = 0; j < map.GridTileHeight; j++)
            {
                Vector2Int tile = new Vector2Int(i, j);
                if ((tile - _effector.GridPosition).magnitude <= 5 &&
                    (tile - _chosenAlly.GridPosition).magnitude <= _chosenAlly.AllyCharacter.RangeShot)
                {
                    _possibleTargetsTiles.Add(map[i, j]);
                }
            }
        }
        //Debug.Log(_possibleTargetsTiles.Count);
        //_targetableTiles.UpdateTileZoneDisplay(_possibleTargetsTiles, TileZoneDisplayEnum.MoveZoneDisplay);
        _targetableTiles.DisplayTileZone("AttackZone", _possibleTargetsTiles, false);


    }

    protected override void EnemyTargetingInput()
    {
        // TODO: Détection des alliés
        // Système de visée ici !
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
        {
            // J'affiche la zone ciblée, en mettant à jour les tiles (ce sont celles situées à portée de la tile ciblée)

            tileCoord = CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point);

            if (tileCoord == _previousTileCoord)
            {
                return;
            }
            if (!_possibleTargetsTiles.Contains(CombatGameManager.Instance.GridMap[tileCoord]))
            {
                //Debug.Log("Taget out of range");
                return;
            }
            else
            {
                _previousTileCoord = tileCoord;

                _areaOfEffectTiles.Clear();

                
                //GridMap map = CombatGameManager.Instance.GridMap;
                _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(tileCoord, _radius);

                CombatGameManager.Instance.TileDisplay.DisplayMouseHoverTileAt(tileCoord);
                _targetableTiles.DisplayTileZone("DamageZone", _areaOfEffectTiles, false);

                // Je parcours la liste des enemis pour récupérer les ennemis ciblés
                // Facultatif ? devra être refait de toute façon - le radius réel est déterminé à  l'Execute()
                // Pour mettre les cibles en surbrillance

                targets.Clear();
                allyTargets.Clear();
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    if ((enemy.GridPosition - tileCoord).magnitude <= _radius) //That's a circle not a diamond...
                    {
                        targets.Add(enemy);
                    }
                }
                foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
                {
                    if ((ally.GridPosition - tileCoord).magnitude <= _radius) //That's a circle not a diamond...
                    {
                        allyTargets.Add(ally);
                    }
                }
                //Debug.Log(targets.Count);
            }
        }
    }

    public override void Execute()
    {
        // Premier test : le Sniper touche-t-il la grenade

        int explosionRadius = 3;

        if (UnityEngine.Random.Range(0, 100) < _allyShotStats.GetAccuracy())
        {
            explosionRadius = 5;
            Debug.Log("[Grenade Toss] Bonus radius");
        }

        targets.Clear();
        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            if ((enemy.GridPosition - tileCoord).magnitude <= explosionRadius) //That's a circle not a diamond...
            {
                targets.Add(enemy); 
            }
        }
        allyTargets.Clear();
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            if ((ally.GridPosition - tileCoord).magnitude <= explosionRadius) //That's a circle not a diamond...
            {
                allyTargets.Add(ally);
            }
        }

        // Ne peux rater ni faire un coup critique
        foreach (EnemyUnit target in targets)
        {
            SelfShoot(target, _selfShotStats, alwaysHit: true, canCrit : false);
        }
        foreach (AllyUnit ally in allyTargets)
        {
            FriendlyFireDamage(_effector, ally, _selfShotStats.GetDamage(), ally.AllyCharacter);
        }
        Debug.Log("[Grenade Toss] Explosion");
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        // The ally must be a Sniper
        // The grenade can be tossed at 5 tiles, and the sniper must be able to shoot it
        return
            //(unit.AllyCharacter.CharacterClass == EnumClasses.Sniper) && ----> à décommenter quand le debug sera fini
            (unit.GridPosition - this._effector.GridPosition).magnitude <= 5 + unit.AllyCharacter.RangeShot;
    }
}
