using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : GridBasedUnit
{
    private BaseAllyAbility _currentAbility = null;
    public BaseAllyAbility CurrentAbility { get { return _currentAbility; } }

    public AllyCharacter AllyCharacter { get => (AllyCharacter)_character; }

    public delegate void EventStartUsingAbility(BaseAllyAbility ability);
    public static event EventStartUsingAbility OnStartedUsingAbility;

    public delegate void EventStopUsingAbility();
    public static event EventStopUsingAbility OnStoppedUsingAbility;

    private new void Awake()
    {
        base.Awake();
        _selectUnitSprite.SetEnabled(true);
        _selectUnitSprite.SetAlly();
    }

    private new void Start()
    {
        base.Start();
        _info.SetCover(CombatGameManager.Instance.GridMap.GetBestCoverAt(GridPosition));
    }

    protected override bool IsEnemy()
    {
        return false;
    }

    //private void OnEnable()
    //{
    //    OnMoveStart += OnMoveStartFunc;
    //    OnMoveFinish += OnMoveFinishFunc;
    //}

    //private void OnDisable()
    //{
    //    OnMoveStart -= OnMoveStartFunc;
    //    OnMoveFinish -= OnMoveFinishFunc;
    //}

    public void UpdateEnemyVisibilities()
    {
        foreach (var pair in _linesOfSight)
        {
            var enemy = (EnemyUnit)pair.Key;
            enemy.UpdateVisibility(true, pair.Value.cover);
        }
    }

    public void UseAbility(BaseAllyAbility ability)
    {
        if (ability == _currentAbility) return;

        _currentAbility = ability;
        ability.SetEffector(this);
        ability.OnAbilityEnded += StopUsingAbility;
        if (OnStartedUsingAbility != null) OnStartedUsingAbility(ability);
    }

    public void UseAbility(int i)
    {
        if (i >= AllyCharacter.Abilities.Count) return;
        UseAbility(AllyCharacter.Abilities[i]);
    }

    //public void UseAbilityAsAlly(BaseAbility ability)
    //{
    //    _currentAbility = ability;
    //    ability.OnAbilityEnded += StopUsingAbility;
    //}

    private void StopUsingAbility(bool executed)
    {
        _currentAbility.OnAbilityEnded -= StopUsingAbility;
        _currentAbility = null;

        if (executed)
        {
            CombatGameManager.Instance.FinishAllyUnitTurn(this);
            _info.SetGreyedBar();
            _selectUnitSprite.SetEnabled(false);
        }

        if (OnStoppedUsingAbility != null) OnStoppedUsingAbility();
    }

    public void StopUsingAbilityAsAlly(bool executed)
    {
        //_currentAbility.OnAbilityEnded -= StopUsingAbility;
        //_currentAbility = null;

        if (executed)
        {
            CombatGameManager.Instance.FinishAllyUnitTurn(this, true);
            _info.SetGreyedBar();
            _selectUnitSprite.SetEnabled(false);
        }
    }

    public override void NewTurn()
    {
        _movesLeft = AllyCharacter.MovementPoints;
        _info.SetColoredBar();
        _selectUnitSprite.SetAlly();
        _selectUnitSprite.SetEnabled(true);
        NeedsPathfinderUpdate();
        UpdateLineOfSights(!IsEnemy());

        foreach (Relationship relationship in AllyCharacter.Relationships.Values)
        {
            relationship.CheckedDuoRefusal = false;
        }
    }

    public override void InitSprite()
    {
        _unitRenderer.sprite = AllyCharacter.GetSprite();
        transform.Find("DeadRenderer").GetComponent<SpriteRenderer>().sprite = GlobalGameManager.Instance.GetDeadAllySprite();
    }

    public void DisplayUnitSelectionTile(bool selected)
    {
        if (selected) _selectUnitSprite.SetAllySelected();
        else _selectUnitSprite.SetAlly();
    }

    //protected void OnMoveStartFunc(GridBasedUnit unit, Vector2Int finalPos)
    //{
    //    if (unit == this && CombatGameManager.Instance.CurrentUnit == this)
    //    {
    //        DisplayUnitSelectionTile(false);
    //    }
    //}

    //protected void OnMoveFinishFunc(GridBasedUnit unit)
    //{
    //    if (unit == this && CombatGameManager.Instance.CurrentUnit == this)
    //    {
    //        DisplayUnitSelectionTile(true);
    //    }
    //}

    public override void MoveToCell(Vector2Int cell, bool eventOnEnd = false)
    {
        base.MoveToCell(cell, eventOnEnd);
        _info.SetCover(CombatGameManager.Instance.GridMap.GetBestCoverAt(cell));
    }

    public void UpdateInfoCover()
    {
        _info.SetCover(CombatGameManager.Instance.GridMap.GetBestCoverAt(GridPosition));
    }
}
