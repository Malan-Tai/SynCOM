using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatInputController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayerMask;

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
        bool clicked = Input.GetMouseButtonUp(0);


        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitUnit = hitData.transform.GetComponent<AllyUnit>();

            if (hitUnit != null && clicked && hitUnit != CombatGameManager.Instance.CurrentUnit)
            {
                CombatGameManager.Instance.SelectControllableUnit(hitUnit);
                changedUnitThisFrame = true;
                clicked = false;
            }
        }
        if (Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
        {
            Vector2Int tileCoord = CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point);
            CombatGameManager.Instance.TileDisplay.DisplayMouseHoverTileAt(tileCoord);

            if (clicked)
            {
                CombatGameManager.Instance.CurrentUnit.ChoosePathTo(tileCoord);
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
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(new FirstAid());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(new Slap());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(new Devouring());
        }
    }
}
