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

    private List<AllyUnit> _allAllyUnits;
    public List<AllyUnit> AllAllyUnits { get { return _allAllyUnits; } }

    public AllyUnit CurrentUnit { get { return _controllableUnits[_currentUnitIndex]; } }

    public BaseAbility CurrentAbility { get { return _controllableUnits[_currentUnitIndex].CurrentAbility; } }

    [SerializeField]
    private List<EnemyUnit> _enemyUnits;

    [SerializeField]
    private CharacterSheet _characterSheet;

    public List<AllyUnit> ControllableUnits { get { return _controllableUnits; } }
    public List<EnemyUnit> EnemyUnits { get { return _enemyUnits; } }

    private Mission _mission;

    private List<Tile> _previousReachableTiles;


    #region Events

    public delegate void NewTurnEvent();
    public static event NewTurnEvent OnNewTurn;

    public delegate void EventSelectUnit(int squadIndex);
    public static event EventSelectUnit OnUnitSelected;

    public class MissionEndEventArgs : EventArgs
    {
        public readonly bool Success;

        public MissionEndEventArgs(bool success)
        {
            Success = success;
        }
    }

    public delegate void MissionEndEvent(MissionEndEventArgs e);
    public static event MissionEndEvent OnMissionEnd;

    #endregion

    private void Start()
    {
        _mission = GlobalGameManager.Instance.CurrentMission;

        _currentUnitIndex = 0;
        _previousReachableTiles = new List<Tile>();

        

        _allAllyUnits = new List<AllyUnit>();
        foreach (AllyUnit unit in _controllableUnits)
        {
            _allAllyUnits.Add(unit);
        }

        InitCharacters();

        if (OnUnitSelected != null) OnUnitSelected(_currentUnitIndex);

        _characterSheet.InitEventsFromCombat();
    }

    private void InitCharacters()
    {
        // TODO : deprecated soon

        int i = 1;
        foreach (AllyUnit ally in _allAllyUnits)
        {
            //changes here
            if (GlobalGameManager.Instance.currentSquad[i-1] == null)
            {
                ally.Character = new AllyCharacter((EnumClasses)i, 20, 2, 65, 10, 15, 20, 4, 60);
            }
            else
            {
                ally.Character = GlobalGameManager.Instance.currentSquad[i];
            }
            
            
            ally.InitSprite();
            i++;
        }

        foreach (AllyUnit ally in _allAllyUnits)
        {
            ally.AllyCharacter.InitializeRelationships();
        }

        foreach (EnemyUnit enemy in _enemyUnits)
        {
            enemy.Character = new EnemyCharacter(20, 2, 65, 10, 15, 20, 4, 60);
            enemy.InitSprite();
        }

        foreach (AllyUnit ally in _allAllyUnits)
        {
            ally.UpdateLineOfSights();
        }

        UpdateVisibilities();
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

        if (OnUnitSelected != null) OnUnitSelected(_currentUnitIndex);
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

            if (OnUnitSelected != null) OnUnitSelected(_currentUnitIndex);
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

            if (OnUnitSelected != null) OnUnitSelected(_currentUnitIndex);
        }
    }

    public void UpdateReachableTiles()
    {
        List<Tile> newReachable = CurrentUnit.GetReachableTiles();
        _tileDisplay.UpdateTileZoneDisplay(newReachable, TileZoneDisplayEnum.MoveZoneDisplay);
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
        // Check mission end
        if (CheckMissionEnd())
        {
            return;
        }

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
        if (OnNewTurn != null) OnNewTurn();

        foreach (AllyUnit unit in _allAllyUnits)
        {
            unit.NewTurn();
            _controllableUnits.Add(unit);
        }

        _currentUnitIndex = 0;
        SelectControllableUnit(0);
    }

    public void UIConfirmAbility()
    {
        if (CurrentAbility != null)
        {
            CurrentAbility.UIConfirm();
        }
    }

    public void UICancelAbility()
    {
        if (CurrentAbility != null)
        {
            CurrentAbility.UICancel();
        }
    }

    public void NewEnemyTurn()
    {

    }

    public void FinishEnemyUnitTurn()
    {
        // Check mission end
        if (CheckMissionEnd())
        {
            return;
        }
    }

    public bool CheckMissionFailure()
    {
        /// TODO Implement other types of mission failures

        bool allAlliesDown = true;
        for (int i = 0; allAlliesDown && i < _allAllyUnits.Count; i++)
        {
            allAlliesDown &= _allAllyUnits[i].Character.HealthPoints <= 0;
        }

        return allAlliesDown;
    }

    public bool CheckMissionSuccess()
    {
        /// TODO Implement other types of mission successes

        bool allEnemiesDown = true;
        for (int i = 0; allEnemiesDown && i < _enemyUnits.Count; i++)
        {
            allEnemiesDown &= _enemyUnits[i].Character.HealthPoints <= 0;
        }

        return allEnemiesDown;
    }

    private bool CheckMissionEnd()
    {
        // Check mission success/failure
        bool success = CheckMissionSuccess();
        bool failure = CheckMissionFailure();
        if (failure || success)
        {
            // Failure is stronger than success, so if the mission fails and succeed at the same time,
            // it is considered as a failure
            MissionEndEventArgs args = new MissionEndEventArgs(success && !failure);
            OnMissionEnd?.Invoke(args);

            return true;
        }

        return false;
    }

    public void AbilityHoverTarget(GridBasedUnit unit)
    {
        if (CurrentAbility == null) return;

        CurrentAbility.HoverPortrait(unit);
    }

    public void UIClickTarget(GridBasedUnit unit)
    {
        if (CurrentAbility == null) return;

        CurrentAbility.UISelectUnit(unit);
    }

#if UNITY_EDITOR
    public void TestKillAllAllies()
    {
        for (int i = 0; i < _allAllyUnits.Count; i++)
        {
            _allAllyUnits[i].Character.Kill();
        }

        CheckMissionEnd();
    }

    public void TestKillAllEnemies()
    {
        for (int i = 0; i < _enemyUnits.Count; i++)
        {
            _enemyUnits[i].Character.Kill();
        }

        CheckMissionEnd();
    }
#endif
}
