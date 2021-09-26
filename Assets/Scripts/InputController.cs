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
            if (hitTile != null && Input.GetMouseButtonUp(0))
            {
                print("clicked tile : " + hitTile.Tile.Coords);
                GameManager.Instance.currentUnit.ChoosePathTo(hitTile.Tile.Coords);
            }
        }
    }
}
