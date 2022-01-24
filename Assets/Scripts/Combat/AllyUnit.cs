using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : GridBasedUnit
{
    [SerializeField] private GameObject _selectUnitSpriteGO;

    private BaseAllyAbility _currentAbility = null;
    public BaseAllyAbility CurrentAbility { get { return _currentAbility; } }

    public AllyCharacter AllyCharacter { get => (AllyCharacter)_character; }

    public delegate void EventStartUsingAbility(BaseAllyAbility ability);
    public static event EventStartUsingAbility OnStartedUsingAbility;

    public delegate void EventStopUsingAbility();
    public static event EventStopUsingAbility OnStoppedUsingAbility;

    protected override bool IsEnemy()
    {
        return false;
    }

    private void OnEnable()
    {
        OnMoveStart += OnMoveStartFunc;
        OnMoveFinish += OnMoveFinishFunc;
    }

    private void OnDisable()
    {
        OnMoveStart -= OnMoveStartFunc;
        OnMoveFinish -= OnMoveFinishFunc;
    }

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
        }
    }

    public override void NewTurn()
    {
        _movesLeft = AllyCharacter.MovementPoints;
        NeedsPathfinderUpdate();
        UpdateLineOfSights(!IsEnemy());

        foreach (Relationship relationship in AllyCharacter.Relationships.Values)
        {
            relationship.CheckedDuoRefusal = false;
        }
    }

    public override void InitSprite()
    {
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = AllyCharacter.GetSprite();
    }

    public void DisplayUnitSelectionTile(bool display)
    {
        _selectUnitSpriteGO.SetActive(display);
    }

    protected void OnMoveStartFunc(GridBasedUnit unit, Vector2Int finalPos)
    {
        if (unit == this && CombatGameManager.Instance.CurrentUnit == this)
        {
            DisplayUnitSelectionTile(false);
        }
    }

    protected void OnMoveFinishFunc(GridBasedUnit unit)
    {
        if (unit == this && CombatGameManager.Instance.CurrentUnit == this)
        {
            DisplayUnitSelectionTile(true);
        }
    }
}
