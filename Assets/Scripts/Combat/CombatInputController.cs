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
            var hitUnit = hitData.transform.GetComponent<AllyUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked)
            {
                CombatGameManager.Instance.SelectControllableUnit(hitUnit);
                changedUnitThisFrame = true;
            }
            else if (hitData.transform.CompareTag("Ground") && clicked)
            {
                CombatGameManager.Instance.CurrentUnit.ChoosePathTo(CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point));
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !changedUnitThisFrame)
        {
            CombatGameManager.Instance.NextControllableUnit();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(new HunkerDown());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(new BasicShot());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(new BasicDuoShot());
        }
    }
}
