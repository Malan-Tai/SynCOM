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

    private void Awake()
    {
        InterruptionQueue = GetComponent<InterruptionQueue>();
        _renderer = GetComponentInChildren<GeneralRenderer>();
        _basicEnemyShot = new BasicEnemyShot();
        _basicEnemyShot.SetEffector(this);
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
        }
        else
        {
            switch (cover)
            {
                case EnumCover.None:
                    _renderer.SetColor(Color.green);
                    break;
                case EnumCover.Half:
                    _renderer.SetColor(Color.yellow);
                    break;
                case EnumCover.Full:
                    _renderer.SetColor(Color.red);
                    break;
                default:
                    _renderer.RevertToOriginalColor();
                    break;
            }
        }
    }

    public override void InitSprite()
    {
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = GlobalGameManager.Instance.GetEnemySprite();
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
