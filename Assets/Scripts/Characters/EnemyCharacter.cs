using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character
{
    public EnemyCharacter(float maxHealth, float damage, float accuracy, float dodge, float critChances, float rangeShot, float movementPoints, float weight) :
        base(maxHealth, damage, accuracy, dodge, critChances, rangeShot, movementPoints, weight)
    {

    }

    public override Sprite GetSprite()
    {
        return GlobalGameManager.Instance.GetEnemySprite();
    }

    public override Sprite GetPortrait()
    {
        return GlobalGameManager.Instance.GetEnemyPortrait();
    }
}
