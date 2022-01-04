using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mortar : BaseDuoAbility
{
    private LayerMask _groundLayerMask = LayerMask.GetMask("Ground");
    private TileDisplay _target;
    List<Tile> _areaOfEffectTiles = new List<Tile>();
    private Vector2Int _previousTileCoord;
    List<EnemyUnit> targets = new List<EnemyUnit>();

    private int _radius = 2;

    public Mortar()
    {
        GameObject map = GameObject.Find("Map");
        if (map == null)
        {
            Debug.Log("GameObject [Map] not found");
        }
        else
        {
            _target = map.transform.Find("TileDisplay").GetComponent<TileDisplay>();
        }
    }

    public override string GetAllyDescription()
    {
        return "Send a beacon in the air to indicate your position. You take cover but have a small chance to get hit.";
    }
    public override string GetDescription()
    {
        return "Fire a splinter-filled on you ally's position, hoping they’ll take cover in time.";
    }
    public override string GetName()
    {
        return "Mortar";
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null && targets.Count != 0;
    }

    protected override void ChooseAlly()
    {
        
    }

    protected override void EnemyTargetingInput()
    {
        ///Système de visée ici !
        ///
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
        {
            // J'affiche la zone ciblée, en mettant à jour les tiles (ce sont celles situées à portée de la tile ciblée)
            

            Vector2Int tileCoord = CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point);

            if (tileCoord == _previousTileCoord)
            {
                return;
            }
            else
            {
                _previousTileCoord = tileCoord;

                _areaOfEffectTiles.Clear();

                // TODO: Faire une fonction : public List<Tile> GetAreaOfEffet[Shape](Vector2Int center, [additional params : int radius, etc.])
                //       et qui prend en compte les bords de la Map pour éviter d'ajouter des Tiles qui n'existent pas.

                //_areaOfEffectTiles = GetAreaOfEffectDiamond(tileCoord, 3);
                GridMap map = CombatGameManager.Instance.GridMap;
                _areaOfEffectTiles.Add(map[tileCoord - new Vector2Int(2, 0)]);
                _areaOfEffectTiles.Add(map[tileCoord - new Vector2Int(1, 0)]);
                _areaOfEffectTiles.Add(map[tileCoord + new Vector2Int(2, 0)]);
                _areaOfEffectTiles.Add(map[tileCoord + new Vector2Int(1, 0)]);
                _areaOfEffectTiles.Add(map[tileCoord - new Vector2Int(0, 2)]);
                _areaOfEffectTiles.Add(map[tileCoord - new Vector2Int(0, 1)]);
                _areaOfEffectTiles.Add(map[tileCoord + new Vector2Int(0, 2)]);
                _areaOfEffectTiles.Add(map[tileCoord + new Vector2Int(0, 1)]);
                _areaOfEffectTiles.Add(map[tileCoord]);

                CombatGameManager.Instance.TileDisplay.DisplayMouseHoverTileAt(tileCoord);
                //_target.UpdateTileZoneDisplay(_areaOfEffectTiles, TileZoneDisplayEnum.AttackZoneDisplay);
                _target.DisplayTileZone("DamageZone", _areaOfEffectTiles, false);

                // Je parcours la liste des enemis pour récupérer les ennemis ciblés

                targets.Clear();
                foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
                {
                    if ( (enemy.GridPosition - tileCoord).magnitude <= _radius )
                    {
                        targets.Add(enemy);
                    }
                }

                Debug.Log(targets.Count);
            }
        }
    }

    public override void Execute()
    {
        
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return true;
    }
}
