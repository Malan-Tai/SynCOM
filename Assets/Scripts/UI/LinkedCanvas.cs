using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedCanvas : MonoBehaviour
{
    [SerializeField]
    private LinkedCanvas _left;

    [SerializeField]
    private LinkedCanvas _right;

    [SerializeField]
    private LinkedCanvas _up;

    [SerializeField]
    private LinkedCanvas _down;

    public RectTransform RectTransform
    {
        get
        {
            return this.GetComponent<RectTransform>();
        }
    }

    public LinkedCanvas GetNextCanvas(int x, int y)
    {
        if (Mathf.Abs(x + y) > 1) return null;

        if (x > 0) return _right;
        if (x < 0) return _left;
        if (y > 0) return _up;
        if (y < 0) return _down;

        return null;
    }
}
