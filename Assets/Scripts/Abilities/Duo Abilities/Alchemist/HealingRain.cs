using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingRain : BaseDuoAbility
{
    private LayerMask _groundLayerMask = LayerMask.GetMask("Ground");

    private Vector2Int _previousTileCoord;
    private Vector2Int _tileCoord;

    private List<EnemyUnit> _enemyTargets = new List<EnemyUnit>();
    private List<AllyUnit> _allyTargets = new List<AllyUnit>();

    private List<Tile> _areaOfEffectTiles = new List<Tile>();
    private List<Tile> _areaOfEffectBonusTiles = new List<Tile>();
    private List<Tile> _possibleTargetsTiles = new List<Tile>();

    // Utilisé pour déterminer l'accuracy de l'allié
    private AbilityStats _allyShotStats;
    private AbilityStats _selfHealStats;

    private int _healingValue = 8;
    private int _healingValueIncreased = 12;

    private int _explosionBaseRadius = 3;
    private int _explosionImprovedRadius = 5;
    private int _trowingRadius = 10;

    public override bool CanExecute()
    {
        return _chosenAlly != null
            && (_tileCoord - _effector.GridPosition).magnitude <= _trowingRadius
            && (_tileCoord - _chosenAlly.GridPosition).magnitude <= _chosenAlly.AllyCharacter.RangeShot;
    }

    public override void Execute()
    {
        // Premier test : le Sniper touche-t-il la grenade

        int explosionRadius = _explosionBaseRadius;
        int healingValue = _healingValue;
        AbilityResult result = new AbilityResult();

        if (!StartAction(ActionTypes.Attack, _chosenAlly, _effector))
        {
            if (RandomEngine.Instance.Range(0, 100) < _allyShotStats.GetAccuracy())
            {
                explosionRadius = _explosionImprovedRadius;
                healingValue = _healingValueIncreased;
                result.Critical = true;
                Debug.Log("[Healing Rain] Bonus radius");
            }
        }
        else
        {
            result.AllyCancelled = true;
        }

        _selfHealStats = new AbilityStats(0, 0, 0, 0, healingValue, _effector);
        _selfHealStats.UpdateWithEmotionModifiers(_chosenAlly);

        _enemyTargets.Clear();
        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            //if ((enemy.GridPosition - tileCoord).magnitude <= explosionRadius) //That's a circle not a diamond...
            if (Mathf.Abs(enemy.GridPosition.x - _tileCoord.x) + Mathf.Abs(enemy.GridPosition.y - _tileCoord.y) <= explosionRadius)
            {
                _enemyTargets.Add(enemy);
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
        foreach (AllyUnit ally in _allyTargets)
        {
            result.HealList.Add(Heal(_effector, ally, _selfHealStats.GetHeal(), _chosenAlly));
        }
        foreach (EnemyUnit enemy in _enemyTargets)
        {
            float heal = _selfHealStats.GetHeal();
            enemy.Heal(ref heal);
            result.HealList.Add(heal);
        }

        SoundManager.PlaySound(SoundManager.Sound.HealingRain);
        SendResultToHistoryConsole(result);
        Debug.Log("[Healing Rain] Explosion");
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        string criticalText = result.Critical ? " greatly" : "";

        if (result.AllyCancelled)
        {
            HistoryConsole.Instance
                .BeginEntry()
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
                .AddText(" tried to use ")
                .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
                .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
                .AddText(" with ")
                .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
                .AddText(" who ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText("cancelled").CloseTag()
                .AddText(" his action to do something else... ")
                .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name.Split(' ')[0]).CloseTag()
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($" still{criticalText} healed ").CloseTag();
        }
        else
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
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{criticalText} healed ").CloseTag();
        }

        List<GridBasedUnit> everyTarget = new List<GridBasedUnit>();
        everyTarget.AddRange(_allyTargets);
        everyTarget.AddRange(_enemyTargets);

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

            string name = everyTarget[i].Character.Name;
            if (everyTarget[i].Character.Name == _effector.Character.Name || everyTarget[i].Character.Name == _chosenAlly.Character.Name)
            {
                name = name.Split(' ')[0];
            }

            HistoryConsole.Instance
                .OpenLinkTag(everyTarget[i].Character.Name, everyTarget[i], EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(name).CloseTag()
                .AddText(" for ")
                .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText(result.HealList[i].ToString()).CloseTag();
        }

        HistoryConsole.Instance.Submit();
    }

    public override string GetAllyDescription()
    {
        string res = "You shoot the vial midair with you expert precision. If you succeed, " +
                     "the vial benefits from an increased healing radius and efficiency.";
        if (_chosenAlly != null)
        {
            res += "\nACC: " + (int)_allyShotStats.GetAccuracy() + "%";
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporaryAllyShotStat = new AbilityStats(0, 0, 0, 0, 0, _temporaryChosenAlly);
            temporaryAllyShotStat.UpdateWithEmotionModifiers(_effector);

            res += "\nACC: " + (int)temporaryAllyShotStat.GetAccuracy() + "%";
        }
        return res;
    }

    public override string GetDescription()
    {
        string res = "You throw a vial filled with a healing concoction in the air for your ally to shoot at.";
        if (_chosenAlly != null)
        {
            res += "\nHEAL: " + (int)_selfHealStats.GetHeal();
        }
        else if (_temporaryChosenAlly != null)
        {
            var temporarySelfHeal = new AbilityStats(0, 0, 0, 0, _healingValue, _effector);
            temporarySelfHeal.UpdateWithEmotionModifiers(_temporaryChosenAlly);

            res += "\nHEAL: " + (int)temporarySelfHeal.GetHeal();
        }
        else
        {
            res += "\nHEAL: " + _healingValue;
        }
        return res;
    }
    public override string GetName()
    {
        return "Healing Rain";
    }

    public override string GetShortDescription()
    {
        return "Throws a healing vial and lets an ally shoot at it for increased efficiency.";
    }

    protected override void ChooseAlly()
    {
        _allyShotStats = new AbilityStats(0, 0, 0, 0, 0, _chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);
        _selfHealStats = new AbilityStats(0, 0, 0, 0, _healingValue, _effector);
        _selfHealStats.UpdateWithEmotionModifiers(_chosenAlly);

        RequestDescriptionUpdate();

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

        if (!BlockingUIElement.IsUIHovered && Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
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

                CombatGameManager.Instance.TileDisplay.DisplayTileZone("BonusHealZone", _areaOfEffectBonusTiles, false);
                CombatGameManager.Instance.TileDisplay.DisplayTileZone("HealZone", _areaOfEffectTiles, false);

                // Je cache le highlight des anciennes targets
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    enemy.DontHighlightUnit();
                }
                foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
                {
                    ally.DontHighlightUnit();
                }

                // Je parcours la liste des alliés pour récupérer les alliés ciblés
                // Devra être refait de toute façon - le radius réel est déterminé à  l'Execute()
                // Pour mettre les cibles en surbrillance
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    //if ((enemy.GridPosition - tileCoord).magnitude <= _radius) //That's a circle not a diamond...
                    if (Mathf.Abs(enemy.GridPosition.x - _tileCoord.x) + Mathf.Abs(enemy.GridPosition.y - _tileCoord.y) <= _explosionBaseRadius)
                    {
                        _enemyTargets.Add(enemy);
                        enemy.HighlightUnit(Color.green);
                    }
                }
                foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
                {
                    //if ((ally.GridPosition - tileCoord).magnitude <= _radius) //That's a circle not a diamond...
                    //Debug.Log(Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y));
                    if (Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y) <= _explosionBaseRadius)
                    {
                        _allyTargets.Add(ally);
                        ally.HighlightUnit(Color.green);
                    }
                }
            }
        }
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= _trowingRadius + unit.AllyCharacter.RangeShot;
    }

    protected override void EndAbility()
    {
        base.EndAbility();

        // Je cache le highlight des anciennes targets
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            ally.DontHighlightUnit();
        }
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
                if ((tile - user.GridPosition).magnitude <= _trowingRadius)
                {
                    range.Add(map[i, j]);
                }
            }
        }
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("AttackZone", range, true);
    }
}
