using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    private RectTransform _rect;

    [SerializeField] private Vector2 _targetPosition = new Vector2(-10f, -10f);
    [SerializeField] private float _targetScale = 0.27f;
    [SerializeField, Range(0.1f, 5f)] private float _time = 1f;

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
        WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

        float progress = 0f;
        Vector2 startPosition = _rect.anchoredPosition;
        Vector3 startScale = _rect.localScale;
        Vector3 targetScale = new Vector3(_targetScale, _targetScale, 1f);
        while (progress < 1f)
        {
            _rect.anchoredPosition = Vector2.Lerp(startPosition, _targetPosition, progress);
            _rect.localScale = Vector3.Lerp(startScale, targetScale, progress);

            progress += Time.fixedDeltaTime / _time;
            yield return _waitForFixedUpdate;
        }

        _rect.anchoredPosition = _targetPosition;
        _rect.localScale = targetScale;

        if (OnScalingDone != null) OnScalingDone();
    }
}
