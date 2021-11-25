using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmCancelAbility : MonoBehaviour
{
    private const float OFFSET_Y = -1000;

    private Vector3 _basePosition;

    private bool _hidden;

    private void Awake()
    {
        _basePosition = this.transform.localPosition;
        this.transform.localPosition += new Vector3(0, OFFSET_Y, 0);
        _hidden = true;
    }

    private void OnEnable()
    {
        AllyUnit.OnStartedUsingAbility += StartUsing;
        AllyUnit.OnStoppedUsingAbility += StopUsing;
    }

    private void OnDisable()
    {
        AllyUnit.OnStartedUsingAbility -= StartUsing;
        AllyUnit.OnStoppedUsingAbility -= StopUsing;
    }

    private void StartUsing(BaseAbility ability)
    {
        if (_hidden)
        {
            //this.transform.position -= new Vector3(0, OFFSET_Y, 0);
            this.transform.localPosition = _basePosition;
            _hidden = false;
        }
    }

    private void StopUsing()
    {
        if (_hidden) return;

        this.transform.position += new Vector3(0, OFFSET_Y, 0);
        _hidden = true;
    }
}
