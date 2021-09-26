using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitTile = hitData.transform.GetComponent<TileComponent>();
            var hitUnit = hitData.transform.GetComponent<GridBasedUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked)
            {
                GameManager.Instance.SelectControllableUnit(hitUnit);
            }
            else if (hitTile != null && clicked)
            {
                GameManager.Instance.CurrentUnit.ChoosePathTo(hitTile.Tile.Coords);
            }
        }
    }
}
