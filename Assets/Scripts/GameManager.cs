using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

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

    [SerializeField]
    private List<GridBasedUnit> _controllableUnits;
    private int _currentUnitIndex;
    public GridBasedUnit CurrentUnit { get { return _controllableUnits[_currentUnitIndex]; } }

    private void Start()
    {
        _currentUnitIndex = 0;
    }

    public void NextControllableUnit()
    {
        _currentUnitIndex++;
        if (_currentUnitIndex >= _controllableUnits.Count) _currentUnitIndex = 0;
        _camera.SwitchParenthood(CurrentUnit);
    }

    public void SelectControllableUnit(GridBasedUnit unit)
    {
        int index = _controllableUnits.IndexOf(unit);
        if (index >= 0 && index < _controllableUnits.Count)
        {
            _currentUnitIndex = index;
            _camera.SwitchParenthood(unit);
        }
    }
}
