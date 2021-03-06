using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : GridBasedUnit
{
    private GeneralRenderer _renderer;

    public bool IsMakingTurn { get; protected set; }
    public bool IsTurnDone { get; protected set; }
    public EnemyCharacter EnemyCharacter { get => (EnemyCharacter)_character; }

    private BasicEnemyShot _basicEnemyShot;

    private new void Awake()
    {
        base.Awake();
        InterruptionQueue = GetComponent<InterruptionQueue>();
        _renderer = GetComponentInChildren<GeneralRenderer>();
        _basicEnemyShot = new BasicEnemyShot();
        _basicEnemyShot.SetEffector(this);

        _selectUnitSprite.SetEnemy();
    }

    protected override void Update()
    {
        base.Update();

        if (Character.IsAlive && IsMakingTurn && InterruptionQueue.IsEmpty())
        {
            /// TODO select best ability to use depending on priorities

            _basicEnemyShot.CalculateBestTarget();

            if (_basicEnemyShot.CanExecute())
            {
                //var parameters = new InterruptionParameters
                //{
                //    interruptionType = InterruptionType.FocusTargetForGivenTime,
                //    target = _basicEnemyShot.BestTarget,
                //    time = Interruption.FOCUS_TARGET_TIME
                //};
                //InterruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parameters));

                _basicEnemyShot.Execute();
            }
            else
            {
                //HistoryConsole.AddEntry(EntryBuilder.GetSkipTurnEntry(this));
            }

            IsMakingTurn = false;
        }

        if (!IsMakingTurn && InterruptionQueue.IsEmpty())
        {
            IsTurnDone = true;
            foreach (GridBasedUnit unit in CombatGameManager.Instance.DeadUnits)
            {
                unit.MarkForDeath();
            }
        }
    }

    protected override bool IsEnemy()
    {
        return true;
    }

    public void UpdateVisibility(bool seen, EnumCover cover = EnumCover.Full)
    {
        if (!seen)
        {
            _renderer.SetColor(Color.black);
            _info.HideCover();
        }
        else
        {
            _renderer.RevertToOriginalColor();
            _info.SetCover(cover);
        }
    }

    public override void InitSprite()
    {
        _unitRenderer.sprite = GlobalGameManager.Instance.GetEnemySprite();
        _info.SetEnemy();
        transform.Find("DeadRenderer").GetComponent<SpriteRenderer>().sprite = GlobalGameManager.Instance.GetDeadEnemySprite();
    }

    public void DisplayUnitSelectionTile(bool display)
    {
        _selectUnitSprite.SetEnabled(display);
    }

    public override void NewTurn()
    {
        base.NewTurn();

        IsMakingTurn = Character.IsAlive;
        IsTurnDone = !Character.IsAlive;

        if (IsMakingTurn)
        {
            var parameters = new InterruptionParameters
            {
                interruptionType = InterruptionType.FocusTargetForGivenTime,
                target = this,
                time = Interruption.FOCUS_TARGET_TIME
            };
            InterruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parameters));
        }
    }
}
