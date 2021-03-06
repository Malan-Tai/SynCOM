using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatInputController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayerMask;

    private GridBasedUnit _prevHovered = null;
    private bool _canInitInput;
    private bool _canInput;

    [SerializeField] private GameObject _pauseMenu;

    private void Awake()
    {
        _canInitInput = false;
        _canInput = true;
        Objective.OnScalingDone += CanMove;
    }

    private void CanMove()
    {
        _canInitInput = true;
        Objective.OnScalingDone -= CanMove;
    }

    public void SetInputPossibility(bool canInput)
    {
        _canInput = canInput;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && CombatGameManager.Instance.CurrentAbility == null)
        {
            _canInput = _pauseMenu.activeSelf;
            _pauseMenu.SetActive(!_pauseMenu.activeSelf);
        }

        if (!_canInput || !_canInitInput || CombatGameManager.Instance.ControllableUnits.Count <= 0) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (!BlockingUIElement.IsUIHovered && Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground"))
        {
            Vector2Int tileCoord = CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point);
            CombatGameManager.Instance.TileDisplay.DisplayMouseHoverTileAt(tileCoord);
        }

        GridBasedUnit hitUnit = null;
        if (_prevHovered != null)
        {
            _prevHovered.DisplayOutline(false);
            _prevHovered.InfoSetSmall(false);
        }
        if (!BlockingUIElement.IsUIHovered && Physics.Raycast(ray, out hitData, 1000))
        {
            hitUnit = hitData.transform.GetComponent<GridBasedUnit>();

            if (hitUnit != null)
            {
                hitUnit.DisplayOutline(true);
                hitUnit.InfoSetBig(false);
            }
        }
        _prevHovered = hitUnit;

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

        float x = 0f;
        float y = 0f;

        if (Input.mousePosition.x <= 0 || Input.GetKey(KeyCode.Q))
        {
            x = -1f;
        }
        else if (Input.mousePosition.x >= Screen.width - 1 || Input.GetKey(KeyCode.D))
        {
            x = 1f;
        }
        if (Input.mousePosition.y <= 0 || Input.GetKey(KeyCode.S))
        {
            y = -1f;
        }
        else if (Input.mousePosition.y >= Screen.height - 1 || Input.GetKey(KeyCode.Z))
        {
            y = 1f;
        }
        CombatGameManager.Instance.Camera.EdgeMove(x, y);
    }

    private void BaseInputControl()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        bool changedUnitThisFrame = false;
        bool clicked = Input.GetMouseButtonUp(0);


        if (!BlockingUIElement.IsUIHovered && Physics.Raycast(ray, out hitData, 1000))
        {
            var hitUnit = hitData.transform.GetComponent<AllyUnit>();

            if (hitUnit != null && clicked && hitUnit != CombatGameManager.Instance.CurrentUnit)
            {
                CombatGameManager.Instance.SelectControllableUnit(hitUnit);
                changedUnitThisFrame = true;
                clicked = false;
            }
        }
        if (!BlockingUIElement.IsUIHovered && Physics.Raycast(ray, out hitData, 1000, _groundLayerMask) && hitData.transform.CompareTag("Ground") && clicked)
        {
            Vector2Int tileCoord = CombatGameManager.Instance.GridMap.WorldToGrid(hitData.point);
            //CombatGameManager.Instance.TileDisplay.DisplayMouseHoverTileAt(tileCoord);
            if (tileCoord != CombatGameManager.Instance.CurrentUnit.GridPosition)
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
            CombatGameManager.Instance.CurrentUnit.UseAbility(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(7);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            CombatGameManager.Instance.CurrentUnit.UseAbility(8);
        }
    }
}
