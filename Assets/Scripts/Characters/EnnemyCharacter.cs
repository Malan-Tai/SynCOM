using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyCharacter : Character
{
    public override Sprite GetSprite()
    {
        return GlobalGameManager.Instance.GetEnemySprite();
    }

    public override Sprite GetPortrait()
    {
        return GlobalGameManager.Instance.GetEnemyPortrait();
    }
}
