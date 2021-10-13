using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GeneralRenderer : MonoBehaviour
{
    protected Color _originalColor;

    public abstract void SetColor(Color color);

    public void RevertToOriginalColor()
    {
        SetColor(_originalColor);
    }
}
