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
    private List<Tile> _areaOfEffectBonusTiles = new List<Tile>();
    private List<Tile> _possibleTargetsTiles = new List<Tile>();

    // Utilisé pour déterminer l'accuracy de l'allié
    private AbilityStats _allyShotStats;
    // Utilisé pour déterminer les dégâts infligés
    private AbilityStats _selfShotStats;

    private int _explosionBaseRadius = 3;
    private int _explosionImprovedRadius = 5;
    private int _throwingRadius = 10;

    public override string GetAllyDescription()
    {
        string res =  "You shoot the grenade midair with you expert precision. If you succeed, " +
                      "the grenade benefits from an increased explosion radius and damage.";
        if (_chosenAlly != null)
        {
            res += "ACC:" + (int)_allyShotStats.GetAccuracy() + "%";
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporaryAllyShotStat = new AbilityStats(0, 0, 0, 0, 0, _temporaryChosenAlly);
            temporaryAllyShotStat.UpdateWithEmotionModifiers(_effector);

            res += "ACC:" + (int)temporaryAllyShotStat.GetAccuracy() + "%";
        }
        res += "\nBONUS DMG: +33%";
        return res;
    }

    public override string GetDescription()
    {
        string res = "You throw a grenade in the air for the Sniper to shoot at.";
        if (_chosenAlly != null)
        {
            res += "\nACC: 100%" +
                    " | CRIT: 0%" +
                    " | DMG: " + (int)_selfShotStats.GetDamage();
        }
        else if (_effector != null & _temporaryChosenAlly != null)
        {
            var temporarySelfShotStat = new AbilityStats(0, 0, 1.5f, 0, 0, _effector);
            temporarySelfShotStat.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nACC: 100%" +
                    " | CRIT: 0%" +
                    " | DMG: " + (int)temporarySelfShotStat.GetDamage();
        }
        else
        {
            res += "\nACC: 100%" +
                    " | CRIT: 0%";
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
            && (_tileCoord - _effector.GridPosition).magnitude <= _throwingRadius
            && (_tileCoord - _chosenAlly.GridPosition).magnitude <= _chosenAlly.AllyCharacter.RangeShot;
    }

    protected override void ChooseAlly()
    {
        _allyShotStats = new AbilityStats(0, 0, 0, 0, 0, _chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);

        _selfShotStats = new AbilityStats(0, 0, 1.5f, 0, 0, _effector);
        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);

        _possibleTargetsTiles.Clear();
        GridMap map = CombatGameManager.Instance.GridMap;
        for (int i = 0; i < map.GridTileWidth; i++)
        {
            for (int j = 0; j < map.GridTileHeight; j++)
            {
                Vector2Int tile = new Vector2Int(i, j);
                if ((tile - _effector.GridPosition).magnitude <= _throwingRadius &&
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

        // Nouveau :
        // - l'AoE suit le souris
        // - quand je clique, lance UIConfirm()

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

                //CombatGameManager.Instance.CameraPointer.MoveToCell(_tileCoord);
                //CombatGameManager.Instance.Camera.SwitchParenthood(CombatGameManager.Instance.CameraPointer);

                //Debug.Log("Target : " + tileCoord
                //        + " + | Ally : " + _chosenAlly.GridPosition
                //        + " + | Effector : " + _effector.GridPosition);

                _areaOfEffectTiles.Clear();
                _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_tileCoord, _explosionBaseRadius);
                _areaOfEffectBonusTiles.Clear();
                _areaOfEffectBonusTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_tileCoord, _explosionImprovedRadius);

                CombatGameManager.Instance.TileDisplay.DisplayTileZone("BonusDamageZone", _areaOfEffectBonusTiles, false);
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
                        enemy.HighlightUnit(Color.red);
                    }
                }
                foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
                {
                    //if ((ally.GridPosition - tileCoord).magnitude <= _radius) //That's a circle not a diamond...
                    //Debug.Log(Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y));
                    if ( Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y) <= _explosionBaseRadius)
                    {
                        _allyTargets.Add(ally);
                        ally.HighlightUnit(Color.red);
                    }
                }

                
            }
        }
    }

    public override void Execute()
    {
        // Premier test : le Sniper touche-t-il la grenade

        int explosionRadius = _explosionBaseRadius;

        if (!StartAction(ActionTypes.Attack, _chosenAlly, _effector) && RandomEngine.Instance.Range(0, 100) < _allyShotStats.GetAccuracy())
        {
            explosionRadius = _explosionImprovedRadius;
            Debug.Log("[Grenade Toss] Bonus radius");
            _selfShotStats = new AbilityStats(0, 0, 2f, 0, 0, _effector);
            _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);
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

        AbilityResult result = new AbilityResult();
        result.Damage = _selfShotStats.GetDamage();
        SendResultToHistoryConsole(result);
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
            .AddText(" and ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
            .AddText(" used ")
            .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .AddText(":")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($" did ").CloseTag()
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Damage} damage").CloseTag()
            .AddText(" to ");

        List<GridBasedUnit> everyTarget = new List<GridBasedUnit>();
        everyTarget.AddRange(_targets);
        everyTarget.AddRange(_allyTargets);

        for (int i = 0; i < everyTarget.Count; i++)
        {
            if (i != 0)
            {
                if (i == everyTarget.Count - 1)
                {
                    HistoryConsole.Instance.AddText(" and ");
                }
                else
                {
                    HistoryConsole.Instance.AddText(", ");
                }
            }

            HistoryConsole.Instance
                .OpenLinkTag(everyTarget[i].Character.Name, everyTarget[i], EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(everyTarget[i].Character.Name).CloseTag();
        }

        HistoryConsole.Instance.Submit();
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        // The ally must be a Sniper
        // The grenade can be tossed at 5 tiles, and the sniper must be able to shoot it
        return
            //(unit.AllyCharacter.CharacterClass == EnumClasses.Sniper) && ----> à décommenter quand le debug sera fini
            (unit.GridPosition - this._effector.GridPosition).magnitude <= _throwingRadius + unit.AllyCharacter.RangeShot;
    }

    protected override void EndAbility()
    {
        base.EndAbility();

        // Je cache le highlight des anciennes targets
        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            enemy.DontHighlightUnit();
        }
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            ally.DontHighlightUnit();
        }

        CombatGameManager.Instance.TileDisplay.HideTileZone("DamageZone");
        CombatGameManager.Instance.TileDisplay.HideTileZone("AttackZone");
    }

    public override string GetShortDescription()
    {
        return "Throws a grenade and lets an ally shoot at it for increased efficiency.";
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
                if ((tile - user.GridPosition).magnitude <= _throwingRadius)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
