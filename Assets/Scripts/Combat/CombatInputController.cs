using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatInputController : MonoBehaviour
{
    void Update()
    {
        if (CombatGameManager.Instance.ControllableUnits.Count <= 0) return;

        if (CombatGameManager.Instance.CurrentAbility != null)
        {
            CombatGameManager.Instance.CurrentAbility.InputControl();
        }
        else
        {
            BaseInputControl();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            CombatGameManager.Instance.Camera.RotateCamera(1);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CombatGameManager.Instance.Camera.RotateCamera(-1);
        }

        float scrollY = Input.mouseScrollDelta.y;
        if (scrollY != 0)
        {
            CombatGameManager.Instance.Camera.ZoomCamera(- scrollY);
        }
    }

    private void BaseInputControl()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        bool changedUnitThisFrame = false;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitTile = hitData.transform.GetComponent<TileComponent>();
            var hitUnit = hitData.transform.GetComponent<AllyUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked)
            {
                CombatGameManager.Instance.SelectControllableUnit(hitUnit);
                changedUnitThisFrame = true;
            }
            else if (hitTile != null && clicked)
            {
                CombatGameManager.Instance.CurrentUnit.ChoosePathTo(hitTile.Tile.Coords);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !changedUnitThisFrame)
        {
            CombatGameManager.Instance.NextControllableUnit();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(new HunkerDown());
        }
    }
}
