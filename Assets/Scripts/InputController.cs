using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        bool changedUnitThisFrame = false;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitTile = hitData.transform.GetComponent<TileComponent>();
            var hitUnit = hitData.transform.GetComponent<GridBasedUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked)
            {
                GameManager.Instance.SelectControllableUnit(hitUnit);
                changedUnitThisFrame = true;
            }
            else if (hitTile != null && clicked)
            {
                GameManager.Instance.CurrentUnit.ChoosePathTo(hitTile.Tile.Coords);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !changedUnitThisFrame)
        {
            GameManager.Instance.NextControllableUnit();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            GameManager.Instance.Camera.RotateCamera(1);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.Instance.Camera.RotateCamera(-1);
        }
    }
}
