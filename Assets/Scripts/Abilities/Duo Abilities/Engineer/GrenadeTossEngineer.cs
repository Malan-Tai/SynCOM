using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeTossEngineer : BaseDuoAbility
{
    private LayerMask _groundLayerMask = LayerMask.GetMask("Ground");
    
    private Vector2Int _previousTileCoord;
    private Vector2Int _tileCoord;

    private List<EnemyUnit> _targets = new List<EnemyUnit>();
    private List<AllyUnit> _allyTargets = new List<AllyUnit>();
    
    private List<Tile> _areaOfEffectTiles = new List<Tile>();
    private List<Tile> _possibleTargetsTiles = new List<Tile>();

    // Utilisé pour déterminer l'accuracy de l'allié
    private AbilityStats _allyShotStats;
    // Utilisé pour déterminer les dégâts infligés
    private AbilityStats _selfShotStats;

    private int _explosionBaseRadius = 3;
    private int _explosionImprovedRadius = 5;
    private int _trowingRadius = 10;

    public override string GetAllyDescription()
    {
        return "You shoot the grenade midair with you expert precision. If you succeed, " +
                "the grenade benefits from an increased explosion radius.";
    }

    public override string GetDescription()
    {
        string res = "You throw a grenade in the air for the Sniper to shoot at.";
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
        // - à 10 tiles de l'effector
        // - à porté de l'ally
        //if (_chosenAlly != null)
        //{
        //    Debug.Log("Target : " + tileCoord
        //                   + " + | Ally : " + _chosenAlly.GridPosition
        //                   + " + | Effector : " + _effector.GridPosition);
        //    Debug.Log("distance from effector : " + (tileCoord - _effector.GridPosition).magnitude + " / 10 | distance from ally : " + (tileCoord - _chosenAlly.GridPosition).magnitude + " / " + _chosenAlly.AllyCharacter.RangeShot);
        //}
            return _chosenAlly != null
            && (_tileCoord - _effector.GridPosition).magnitude <= _trowingRadius
            && (_tileCoord - _chosenAlly.GridPosition).magnitude <= _chosenAlly.AllyCharacter.RangeShot;
    }

    protected override void ChooseAlly()
    {
        _allyShotStats = new AbilityStats(0, 0, 0, 0, _chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);

        _selfShotStats = new AbilityStats(0, 0, 1.5f, 0, _effector);
        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);

        _possibleTargetsTiles.Clear();
        GridMap map = CombatGameManager.Instance.GridMap;
        for (int i = 0; i < map.GridTileWidth; i++)
        {
            for (int j = 0; j < map.GridTileHeight; j++)
            {
                Vector2Int tile = new Vector2Int(i, j);
                if ((tile - _effector.GridPosition).magnitude <= _trowingRadius &&
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
        // Système de visée ici !
        // TODO: En trois temps
        // 1- J'affiche le pointeur (DisplayMouseHoverTileAt) au niveau de ma souris
        // 2- Quand je clique gauche, la caméra et la zone de visée se déplacent sur cette tile
        // 3- Quand je clique sur Confirm/appuie sur Entrer, la capacité s'exécute

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
        {
            // J'affiche la zone ciblée, en mettant à jour les tiles (ce sont celles situées à portée de la tile ciblée)

            var temporaryTileCoord = CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point);
            if (!_possibleTargetsTiles.Contains(CombatGameManager.Instance.GridMap[temporaryTileCoord]))
            {
                //Debug.Log("Taget out of range");
                return;
            }
            else
            {
                // La caméra se déplace bien, mais du coup la tile visée se déplace aussi. Voir le TODO plus haut.

                bool clicked = Input.GetMouseButtonUp(0);
                CombatGameManager.Instance.TileDisplay.DisplayMouseHoverTileAt(temporaryTileCoord);
                
                if ((!clicked) || temporaryTileCoord == _previousTileCoord)
                {
                    return;
                }
                _previousTileCoord = temporaryTileCoord;
                _tileCoord = temporaryTileCoord;

                //CombatGameManager.Instance.CameraPointer.MoveToCell(_tileCoord);
                //CombatGameManager.Instance.Camera.SwitchParenthood(CombatGameManager.Instance.CameraPointer);

                //Debug.Log("Target : " + tileCoord
                //        + " + | Ally : " + _chosenAlly.GridPosition
                //        + " + | Effector : " + _effector.GridPosition);

                _areaOfEffectTiles.Clear();

                
                _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_tileCoord, _explosionImprovedRadius);

                
                CombatGameManager.Instance.TileDisplay.DisplayTileZone("DamageZone", _areaOfEffectTiles, false);

                // Je parcours la liste des enemis pour récupérer les ennemis ciblés
                // Facultatif ? devra être refait de toute façon - le radius réel est déterminé à  l'Execute()
                // Pour mettre les cibles en surbrillance

                _targets.Clear();
                _allyTargets.Clear();
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    //if ((enemy.GridPosition - tileCoord).magnitude <= _radius) //That's a circle not a diamond...
                    if (Mathf.Abs(enemy.GridPosition.x - _tileCoord.x) + Mathf.Abs(enemy.GridPosition.y - _tileCoord.y) <= _explosionBaseRadius)
                    {
                        _targets.Add(enemy);
                    }
                }
                foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
                {
                    //if ((ally.GridPosition - tileCoord).magnitude <= _radius) //That's a circle not a diamond...
                    Debug.Log(Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y));
                    if ( Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y) <= _explosionBaseRadius)
                    {
                        _allyTargets.Add(ally);
                    }
                }
            }
        }
    }

    public override void Execute()
    {
        // Premier test : le Sniper touche-t-il la grenade

        int explosionRadius = _explosionBaseRadius;

        if (UnityEngine.Random.Range(0, 100) < _allyShotStats.GetAccuracy())
        {
            explosionRadius = _explosionImprovedRadius;
            Debug.Log("[Grenade Toss] Bonus radius");
        }

        _targets.Clear();
        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            //if ((enemy.GridPosition - tileCoord).magnitude <= explosionRadius) //That's a circle not a diamond...
            if (Mathf.Abs(enemy.GridPosition.x - _tileCoord.x) + Mathf.Abs(enemy.GridPosition.y - _tileCoord.y) <= explosionRadius)
            {
                _targets.Add(enemy); 
            }
        }
        _allyTargets.Clear();
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            //if ((ally.GridPosition - tileCoord).magnitude <= explosionRadius) //That's a circle not a diamond...
            if (Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y) <= explosionRadius)
            {
                _allyTargets.Add(ally);
            }
        }

        // Ne peux rater ni faire un coup critique
        foreach (EnemyUnit target in _targets)
        {
            SelfShoot(target, _selfShotStats, alwaysHit: true, canCrit : false);
        }
        foreach (AllyUnit ally in _allyTargets)
        {
            FriendlyFireDamage(_effector, ally, _selfShotStats.GetDamage(), ally);
        }
        Debug.Log("[Grenade Toss] Explosion");
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        // The ally must be a Sniper
        // The grenade can be tossed at 5 tiles, and the sniper must be able to shoot it
        return
            //(unit.AllyCharacter.CharacterClass == EnumClasses.Sniper) && ----> à décommenter quand le debug sera fini
            (unit.GridPosition - this._effector.GridPosition).magnitude <= _trowingRadius + unit.AllyCharacter.RangeShot;
    }

    protected override void EndAbility()
    {
        base.EndAbility();
        CombatGameManager.Instance.TileDisplay.HideTileZone("DamageZone");
        CombatGameManager.Instance.TileDisplay.HideTileZone("AttackZone");
    }
}
