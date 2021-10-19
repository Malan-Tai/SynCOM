using System;
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
    private GridMap _gridMap;
    public GridMap GridMap { get => _gridMap; }

    [SerializeField]
    private TileDisplay _tileDisplay;
    public TileDisplay TileDisplay { get => _tileDisplay; }

    [SerializeField]
    private MoveableCamera _camera;
    public MoveableCamera Camera { get { return _camera; } }

    [SerializeField]
    private List<AllyUnit> _controllableUnits;
    private int _currentUnitIndex;

    private AllyUnit[] _allAllyUnits;
    public AllyUnit[] AllAllyUnits { get { return _allAllyUnits; } }

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

        _allAllyUnits = new AllyUnit[_controllableUnits.Count];
        _controllableUnits.CopyTo(_allAllyUnits);

        InitCharacters();
    }

    private void InitCharacters()
    {
        foreach (AllyUnit ally in _allAllyUnits)
        {
            ally.Character = new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 4, 60);
        }

        foreach (AllyUnit ally in _allAllyUnits)
        {
            ally.AllyCharacter.InitializeRelationships();
        }

        foreach (EnemyUnit enemy in _enemyUnits)
        {
            enemy.Character = new Character(EnumClasses.Sniper, 20, 2, 65, 10, 15, 4, 60);
        }
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

    public void SelectControllableUnit(int index)
    {
        if (index >= 0 && index < _controllableUnits.Count)
        {
            _currentUnitIndex = index;
            _camera.SwitchParenthood(_controllableUnits[index]);
            UpdateReachableTiles();
            UpdateVisibilities();
        }
    }

    public void UpdateReachableTiles()
    {
        List<Tile> newReachable = CurrentUnit.GetReachableTiles();
        _tileDisplay.DisplayTileZone(newReachable, TileZoneDisplayEnum.MoveZoneDisplay);
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

    public void FinishAllyUnitTurn(AllyUnit unit, bool wasAllyForDuo = false)
    {
        int index = _controllableUnits.IndexOf(unit);
        if (index < 0) return;

        _controllableUnits.Remove(unit);

        if (_controllableUnits.Count <= 0)
        {
            print("end turn");
            NewAllyTurn();
            return;
        }

        if (_currentUnitIndex > index) _currentUnitIndex--;
        if (_currentUnitIndex >= _controllableUnits.Count) _currentUnitIndex = 0;

        if (!wasAllyForDuo) SelectControllableUnit(_currentUnitIndex);
    }

    public void NewAllyTurn()
    {
        print("new turn");

        foreach (AllyUnit unit in _allAllyUnits)
        {
            unit.NewTurn();
            _controllableUnits.Add(unit);
        }

        _currentUnitIndex = 0;
        SelectControllableUnit(0);
    }
}
