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

    public List<GridBasedUnit> DeadUnits { get; private set; }

    [SerializeField]
    private GameObject _barricadePrefab;

    public AllyUnit CurrentUnit
    {
        get
        {
            if (_currentUnitIndex < 0 || _currentUnitIndex >= _controllableUnits.Count)
            {
                return null;
            }

            return _controllableUnits[_currentUnitIndex];
        }
    }

    public BaseAllyAbility CurrentAbility { get { return _controllableUnits[_currentUnitIndex].CurrentAbility; } }

    [SerializeField]
    private List<EnemyUnit> _enemyUnits;
    private int _currentEnemyUnit;

    [SerializeField]
    private CharacterSheet _characterSheet;

    public List<AllyUnit> ControllableUnits { get { return _controllableUnits; } }
    public List<EnemyUnit> EnemyUnits { get { return _enemyUnits; } }

    public bool IsAllyTurn { get; private set; }
    public bool IsEnemyTurn { get; private set; }

    [SerializeField] public Sprite happyEmoji;
    [SerializeField] public Sprite unhappyEmoji;


    #region Events

    public delegate void NewTurnEvent();
    public static event NewTurnEvent OnNewTurn;
    // TODO : for now, OnNewTurn is called on every new ally AND enemy turn, this should be changed

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
        GlobalGameManager.Instance.StartCurrentMission();

        _currentUnitIndex = 0;

        DeadUnits = new List<GridBasedUnit>();

        _allAllyUnits = new List<AllyUnit>();
        foreach (AllyUnit unit in _controllableUnits)
        {
            _allAllyUnits.Add(unit);
        }
        _controllableUnits[_currentUnitIndex].DisplayUnitSelectionTile(true);

        InitCharacters();

        CurrentUnit.InfoSetBig(true);

        if (OnUnitSelected != null) OnUnitSelected(_currentUnitIndex);

        _characterSheet.InitEventsFromCombat();

        IsAllyTurn = true;

        CurrentUnit.DisplayUnitSelectionTile(true);
    }

    private void InitCharacters()
    {
        List<AllyUnit> toRemove = new List<AllyUnit>();

#if UNITY_EDITOR
        bool allNull = true;
        foreach (AllyCharacter charac in GlobalGameManager.Instance.currentSquad)
        {
            if (charac != null)
            {
                allNull = false;
                break;
            }
        }

        if (allNull)
        {
            List<AllyCharacter> characters = new List<AllyCharacter>();

            int i = 0;
            foreach (AllyUnit ally in _allAllyUnits)
            {
                ally.SetCharacter(AllyCharacter.GetRandomAllyCharacter());
                characters.Add(ally.AllyCharacter);
                //ally.AllyCharacter.Name = $"Ally {i+1}";
                i++;
            }

            foreach (AllyUnit ally in _allAllyUnits)
            {
                ally.AllyCharacter.InitializeRelationships(characters);
            }
        }
        else
#endif
        {
        int i = 0;
        foreach (AllyUnit ally in _allAllyUnits)
        {
            //changes here
            if (GlobalGameManager.Instance.currentSquad[i] == null)
            {
                //ally.Character = new AllyCharacter((EnumClasses)i, 20, 2, 65, 10, 15, 20, 4, 60);
                toRemove.Add(ally);
            }
            else
            {
                ally.SetCharacter(GlobalGameManager.Instance.currentSquad[i]);
            }
            i++;
        }
        }

        foreach (AllyUnit unit in toRemove)
        {
            _allAllyUnits.Remove(unit);
            _controllableUnits.Remove(unit);
            Destroy(unit.gameObject);
        }

        int enemyIndex = 1;
        foreach (EnemyUnit enemy in _enemyUnits)
        {
            enemy.SetCharacter(new EnemyCharacter(15, 11, 65, 10, 15, 20, 4, 60));
            enemy.Character.Name = $"Enemy{enemyIndex++}";
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
        GridBasedUnit.OnDeath += UnitDie;
    }

    private void OnDisable()
    {
        GridBasedUnit.OnMoveStart -= UpdatePathfinders;
        GridBasedUnit.OnMoveFinish -= UpdateVisibilities;
        GridBasedUnit.OnDeath -= UnitDie;
    }

    private void Update()
    {
        if (IsAllyTurn) return;

        if (!IsEnemyTurn)
        {
            NewEnemyTurn();
            _enemyUnits[_currentEnemyUnit].NewTurn();
        }

        if (_enemyUnits[_currentEnemyUnit].IsMakingTurn) return;

        if (_enemyUnits[_currentEnemyUnit].IsTurnDone)
        {
            _currentEnemyUnit++;

            if (_currentEnemyUnit == _enemyUnits.Count)
            {
                FinishEnemyUnitTurn();
                NewAllyTurn();
                return;
            }

            _enemyUnits[_currentEnemyUnit].NewTurn();
        }
    }

    public void NextControllableUnit()
    {
        _controllableUnits[_currentUnitIndex].DisplayUnitSelectionTile(false);

        _currentUnitIndex++;
        if (_currentUnitIndex >= _controllableUnits.Count) _currentUnitIndex = 0;

        _controllableUnits[_currentUnitIndex].DisplayUnitSelectionTile(true);
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
            _controllableUnits[_currentUnitIndex].DisplayUnitSelectionTile(false);
            unit.DisplayUnitSelectionTile(true);

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
            _controllableUnits[_currentUnitIndex].DisplayUnitSelectionTile(false);
            _controllableUnits[index].DisplayUnitSelectionTile(true);

            _currentUnitIndex = index;
            _camera.SwitchParenthood(_controllableUnits[index]);
            UpdateReachableTiles();
            UpdateVisibilities();

            if (OnUnitSelected != null) OnUnitSelected(_currentUnitIndex);
        }
        else
        {
            _currentUnitIndex = -1;
        }
    }

    public void UpdateReachableTiles()
    {
        if (CurrentUnit == null)
        {
            _tileDisplay.HideAllTileZones();
            return;
        }

        List<Tile> newReachable = CurrentUnit.GetReachableTiles();
        if (newReachable.Count == 1)
        {
            // Remove the unit tile if it is the only one in the reachable tiles
            newReachable.Clear();
        }

        _tileDisplay.DisplayTileZone("MoveZone", newReachable, true);
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
            foreach (EnemyUnit unit in _enemyUnits)
            {
                unit.UpdateVisibility(false);
            }

            ((AllyUnit)movedUnit).UpdateEnemyVisibilities();
        }
    }

    public void UpdateVisibilities()
    {
        foreach (EnemyUnit unit in _enemyUnits)
        {
            unit.UpdateVisibility(false);
        }

        CurrentUnit.UpdateEnemyVisibilities();
    }

    public void FinishAllyUnitTurn(AllyUnit unit, bool wasAllyForDuo = false)
    {
        Debug.Log("end ally turn so don't display selection tile");
        unit.DisplayUnitSelectionTile(false);

        // Check mission end
        if (CheckMissionEnd())
        {
            print("end");
            return;
        }

        int index = _controllableUnits.IndexOf(unit);
        if (index < 0) return;

        _controllableUnits.Remove(unit);

        if (_controllableUnits.Count <= 0)
        {
            print("end turn");
            IsAllyTurn = false;
            return;
        }

        if (_currentUnitIndex > index) _currentUnitIndex--;
        if (_currentUnitIndex >= _controllableUnits.Count) _currentUnitIndex = 0;

        if (!wasAllyForDuo) SelectControllableUnit(_currentUnitIndex);
    }

    public void NewAllyTurn()
    {
        IsAllyTurn = true;
        print("new ally turn");
        if (OnNewTurn != null) OnNewTurn();

        foreach (AllyUnit unit in _allAllyUnits)
        {
            unit.NewTurn();
            _controllableUnits.Add(unit);
        }

        _currentUnitIndex = 0;
        SelectControllableUnit(0);

        if (_currentUnitIndex > -1) CurrentUnit.DisplayUnitSelectionTile(true);
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
        print("new enemy turn");
        if (OnNewTurn != null) OnNewTurn();

        _currentEnemyUnit = 0;
        IsEnemyTurn = true;
    }

    public void FinishEnemyUnitTurn()
    {
        IsEnemyTurn = false;

        // Check mission end
        if (CheckMissionEnd())
        {
            print("end");
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

    private void UnitDie(GridBasedUnit deadUnit)
    {
        EnemyUnit enemy = deadUnit as EnemyUnit;
        AllyUnit ally = deadUnit as AllyUnit;

        if (enemy != null)
        {
            _enemyUnits.Remove(enemy);
        }
        else if (ally != null)
        {
            GlobalGameManager.Instance.allCharacters.Remove(ally.AllyCharacter);

            _allAllyUnits.Remove(ally);

            int i = _controllableUnits.IndexOf(ally);
            if (i != -1)
            {
                if (i < _currentUnitIndex) _currentUnitIndex--;
                _controllableUnits.Remove(ally);
            }
        }

        //deadUnit.gameObject.SetActive(false);
        _gridMap.FreeOccupiedTile(deadUnit.GridPosition);
        //deadUnit.MarkForDeath();
        DeadUnits.Add(deadUnit);
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

    public void ChangeTileCover(Tile tile, EnumCover cover)
    {
        tile.Cover = cover;
        UpdatePathfinders(null, tile.Coords);
    }

    public void AddBarricadeAt(Vector2Int pos, bool rotate)
    {
        var barricade = Instantiate(_barricadePrefab);
        barricade.transform.position = _gridMap.GridToWorld(pos, 0f);
        if (rotate) barricade.transform.eulerAngles += new Vector3(0, 90, 0);
    }

    public GridBasedUnit GetUnitFromCharacter(Character character)
    {
        foreach (GridBasedUnit unit in _allAllyUnits)
        {
            if (unit.Character == character) return unit;
        }
        foreach (GridBasedUnit unit in _enemyUnits)
        {
            if (unit.Character == character) return unit;
        }
        return null;
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
