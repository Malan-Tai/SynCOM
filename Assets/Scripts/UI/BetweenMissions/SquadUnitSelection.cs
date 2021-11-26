using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadUnitSelection : MonoBehaviour
{
    private const float OFFSET_Y = 2000;
    private Vector3 _basePosition;
    private UnitScrollList _list;

    private void Awake()
    {
        _basePosition = transform.localPosition;
        transform.localPosition += new Vector3(0, OFFSET_Y, 0);
        _list = GetComponentInChildren<UnitScrollList>();
    }

    private void OnEnable()
    {
        _list.OnMouseClickEvent += Hide;
        MissionRecapUnit.OnMouseClickEvent += Show;
    }

    private void OnDisable()
    {
        _list.OnMouseClickEvent += Hide;
        MissionRecapUnit.OnMouseClickEvent += Show;
    }

    private void Show(int i)
    {
        transform.localPosition = _basePosition;
    }

    private void Hide(AllyCharacter character)
    {
        transform.localPosition += new Vector3(0, OFFSET_Y, 0);
    }
}
