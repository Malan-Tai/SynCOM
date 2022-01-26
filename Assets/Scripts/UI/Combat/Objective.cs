using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    private RectTransform _rect;

    [SerializeField] private Vector2 _moveSpeed;
    [SerializeField] private float _scaleSpeed;

    public delegate void DoneScalingEvent();
    public static event DoneScalingEvent OnScalingDone;

    private void Awake()
    {
        _rect = transform as RectTransform;
    }

    private void Start()
    {
        StartCoroutine(ReduceCoroutine());
    }

    private IEnumerator ReduceCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        while (_rect.anchoredPosition.x < -25 && _rect.anchoredPosition.y < -25)
        {
            _rect.anchoredPosition += _moveSpeed;
            _rect.localScale *= _scaleSpeed;

            yield return new WaitForSeconds(0.05f);
        }

        if (OnScalingDone != null) OnScalingDone();
    }
}
