using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smuggle : BaseDuoAbility
{
    private LayerMask _groundLayerMask = LayerMask.GetMask("Ground");

    private Vector2Int _previousTileCoord;
    private Vector2Int _tileCoord;

    private List<Tile> _possibleTargetsTiles = new List<Tile>();

    private int _targetingRadius = 10;

    public override string GetAllyDescription()
    {
        return "Get smuggled";
    }
    public override string GetDescription()
    {
        return "Transport an ally next to you, to take them out of danger or get them in the heat of the battle. " +
               "Don’t ask how they do it - a smuggler has their way.";
    }
    public override string GetName()
    {
        return "Smuggle";
    }
    public override bool CanExecute()
    {
        return _chosenAlly != null &&
               (_chosenAlly.GridPosition - this._effector.GridPosition).magnitude <= _targetingRadius;
    }

    public override void Execute()
    {
        _chosenAlly.ChooseAstarPathTo(_tileCoord);
        _freeForDuo = true;

        SendResultToHistoryConsole(null);
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_effector.Character.Name, _effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_effector.Character.Name).CloseTag()
            .AddText(" used ")
            .OpenIconTag("Duo", EntryColors.ICON_DUO_ABILITY).CloseTag()
            .OpenColorTag(EntryColors.TEXT_ABILITY).AddText(GetName()).CloseTag()
            .AddText(" to transport ")
            .OpenLinkTag(_chosenAlly.Character.Name, _chosenAlly, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_chosenAlly.Character.Name).CloseTag()
            .AddText(" next to him")
            .Submit();
    }

    protected override void ChooseAlly()
    {
        _possibleTargetsTiles.Clear();
        GridMap map = CombatGameManager.Instance.GridMap;
        int x = _effector.GridPosition.x;
        int y = _effector.GridPosition.y;

        Tile tileUp = map[x, y - 1];
        if (tileUp != null) _possibleTargetsTiles.Add(tileUp);
        Tile tileDown = map[x, y + 1];
        if (tileUp != null) _possibleTargetsTiles.Add(tileDown);
        Tile tileRight = map[x + 1, y];
        if (tileUp != null) _possibleTargetsTiles.Add(tileRight);
        Tile tileLeft = map[x - 1, y];
        if (tileUp != null) _possibleTargetsTiles.Add(tileLeft);

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
                // TODO: la case doit être libre
                _previousTileCoord = temporaryTileCoord;
                _tileCoord = temporaryTileCoord;

                //CombatGameManager.Instance.CameraPointer.MoveToCell(_tileCoord);
                //CombatGameManager.Instance.Camera.SwitchParenthood(CombatGameManager.Instance.CameraPointer);
            }
        }
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= _targetingRadius;
    }

    public override string GetShortDescription()
    {
        return "Transports an ally next to you.";
    }
}
