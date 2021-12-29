using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : GridBasedUnit
{
    private GeneralRenderer _renderer;
    private Color _originalColor;

    public EnemyCharacter EnemyCharacter { get => (EnemyCharacter)_character; }

    private BasicEnemyShot _basicEnemyShot;

    private void Awake()
    {
        InterruptionQueue = GetComponent<InterruptionQueue>();
        _renderer = GetComponentInChildren<GeneralRenderer>();
        _basicEnemyShot = new BasicEnemyShot();
        _basicEnemyShot.SetEffector(this);
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

    public void MakeTurn()
    {
        /// TODO select best ability to use depending on priorities

        _basicEnemyShot.CalculateBestTarget();
        
        if (_basicEnemyShot.CanExecute())
        {
            _basicEnemyShot.Execute();
        }
        else
        {
            Debug.Log("No target available: enemy skipping turn.");
        }
    }
}
