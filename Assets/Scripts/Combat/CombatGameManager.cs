using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGameManager : MonoBehaviour
{
    #region Singleton
    private static CombatGameManager instance;
    public static CombatGameManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    [SerializeField]
    public GridMap gridMap;

    [SerializeField]
    private MoveableCamera _camera;
    public MoveableCamera Camera { get { return _camera; } }

    [SerializeField]
    private List<AllyUnit> _controllableUnits;
    private int _currentUnitIndex;

    public AllyUnit CurrentUnit { get { return _controllableUnits[_currentUnitIndex]; } }

    public BaseAbility CurrentAbility { get { return _controllableUnits[_currentUnitIndex].CurrentAbility; } }

    [SerializeField]
    private List<EnemyUnit> _enemyUnits;

    public List<AllyUnit> ControllableUnits { get { return _controllableUnits; } }
    public List<EnemyUnit> EnemyUnits { get { return _enemyUnits; } }

    private List<Tile> _previousReachableTiles;

    private void Start()
    {
        _currentUnitIndex = 0;
        _previousReachableTiles = new List<Tile>();
    }

    private void OnEnable()
    {
        GridBasedUnit.OnMoveStart += UpdatePathfinders;
        GridBasedUnit.OnMoveFinish += UpdateVisibilities;
    }

    private void OnDisable()
    {
        GridBasedUnit.OnMoveStart -= UpdatePathfinders;
        GridBasedUnit.OnMoveFinish -= UpdateVisibilities;
    }

    public void NextControllableUnit()
    {
        _currentUnitIndex++;
        if (_currentUnitIndex >= _controllableUnits.Count) _currentUnitIndex = 0;
        _camera.SwitchParenthood(CurrentUnit);
        UpdateReachableTiles();
        UpdateVisibilities();
    }

    public void SelectControllableUnit(AllyUnit unit)
    {
        int index = _controllableUnits.IndexOf(unit);
        if (index >= 0 && index < _controllableUnits.Count)
        {
            _currentUnitIndex = index;
            _camera.SwitchParenthood(unit);
            UpdateReachableTiles();
            UpdateVisibilities();
        }
    }

    public void UpdateReachableTiles()
    {
        List<Tile> newReachable = CurrentUnit.GetReachableTiles();

        foreach (Tile tile in _previousReachableTiles)
        {
            tile.BecomeUnreachable();
        }

        foreach (Tile tile in newReachable)
        {
            tile.BecomeReachable();
        }

        _previousReachableTiles = newReachable;
    }

    public void UpdatePathfinders(GridBasedUnit movedUnit, Vector2Int finalPos)
    {
        foreach (GridBasedUnit unit in _controllableUnits)
        {
            if (unit != movedUnit)
            {
                unit.NeedsPathfinderUpdate();
            }
        }
    }

    public void UpdateVisibilities(GridBasedUnit movedUnit)
    {
        if (movedUnit == CurrentUnit)
        {
            foreach (GridBasedUnit unit in _enemyUnits)
            {
                ((EnemyUnit)unit).UpdateVisibility(false);
            }

            ((AllyUnit)movedUnit).UpdateEnemyVisibilities();
        }
    }

    public void UpdateVisibilities()
    {
        foreach (GridBasedUnit unit in _enemyUnits)
        {
            ((EnemyUnit)unit).UpdateVisibility(false);
        }

        CurrentUnit.UpdateEnemyVisibilities();
    }
}
