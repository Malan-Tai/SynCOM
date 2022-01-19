using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingRain : BaseDuoAbility
{
    private LayerMask _groundLayerMask = LayerMask.GetMask("Ground");

    private Vector2Int _previousTileCoord;
    private Vector2Int _tileCoord;

    private List<AllyUnit> _allyTargets = new List<AllyUnit>();

    private List<Tile> _areaOfEffectTiles = new List<Tile>();
    private List<Tile> _areaOfEffectBonusTiles = new List<Tile>();
    private List<Tile> _possibleTargetsTiles = new List<Tile>();

    // Utilisé pour déterminer l'accuracy de l'allié
    private AbilityStats _allyShotStats;

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

        if (UnityEngine.Random.Range(0, 100) < _allyShotStats.GetAccuracy())
        {
            explosionRadius = _explosionImprovedRadius;
            healingValue = _healingValueIncreased;
            result.Critical = true;
            Debug.Log("[Healing Rain] Bonus radius");
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
            Heal(_effector, ally, healingValue, _chosenAlly);
        }

        result.Heal = healingValue;
        SendResultToHistoryConsole(result);
        Debug.Log("[Healing Rain] Explosion");
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        string criticalText = result.Critical ? " greatly" : "";

        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
            .AddText(" and ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
            .AddText(" used ")
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .AddText(":")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{criticalText} healed ").CloseTag();

        for (int i = 0; i < _allyTargets.Count; i++)
        {
            if (i != 0)
            {
                if (i == _allyTargets.Count - 1)
                {
                    HistoryConsole.Instance.AddText(" and ");
                }
                else
                {
                    HistoryConsole.Instance.AddText(", ");
                }
            }

            HistoryConsole.Instance
                .OpenLinkTag(_allyTargets[i].Character.Name, _allyTargets[i], EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER)
                .AddText(_allyTargets[i].Character.Name).CloseTag();
        }

        HistoryConsole.Instance
            .AddText(" for ")
            .OpenColorTag(EntryColors.TEXT_IMPORTANT).AddText($"{result.Heal} health points").CloseTag()
            .Submit();
    }

    public override string GetAllyDescription()
    {
        return "You shoot the grenade midair with you expert precision. If you succeed, " +
                "the grenade benefits from an increased explosion radius and efficiency.";
    }
    public override string GetDescription()
    {
        return "You throw a vial filled with a healing concoction in the air for your ally to shoot at." +
               "\nHeal: " + _healingValue;
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
        _allyShotStats = new AbilityStats(0, 0, 0, 0, _chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);

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
                _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_tileCoord, _explosionBaseRadius);
                _areaOfEffectBonusTiles.Clear();
                _areaOfEffectBonusTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_tileCoord, _explosionImprovedRadius);

                CombatGameManager.Instance.TileDisplay.DisplayTileZone("BonusHealZone", _areaOfEffectBonusTiles, false);
                CombatGameManager.Instance.TileDisplay.DisplayTileZone("HealZone", _areaOfEffectTiles, false);

                // Je parcours la liste des alliés pour récupérer les alliés ciblés
                // Devra être refait de toute façon - le radius réel est déterminé à  l'Execute()
                // Pour mettre les cibles en surbrillance

                foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
                {
                    //if ((ally.GridPosition - tileCoord).magnitude <= _radius) //That's a circle not a diamond...
                    Debug.Log(Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y));
                    if (Mathf.Abs(ally.GridPosition.x - _tileCoord.x) + Mathf.Abs(ally.GridPosition.y - _tileCoord.y) <= _explosionBaseRadius)
                    {
                        _allyTargets.Add(ally);
                    }
                }
            }
        }
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= _trowingRadius + unit.AllyCharacter.RangeShot;
    }
}
