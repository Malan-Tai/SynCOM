using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitListElement : MonoBehaviour
{
    private AllyCharacter _character;
    private TMP_Text _unitName;
    private TMP_Text _unitClass;
    private Image _unitPortrait;
    private GameObject _background;

    public delegate void EventMouseEnter(AllyCharacter character);
    public event EventMouseEnter OnMouseEnterEvent;

    public delegate void EventMouseExit();
    public event EventMouseExit OnMouseExitEvent;

    public delegate void EventMouseClick(AllyCharacter character);
    public event EventMouseClick OnMouseClickEvent;

    private bool _needsWidthUpdate = true;
    private bool _frozen = false;

    public void Init()
    {
        _unitName = transform.Find("name").GetComponent<TMP_Text>();
        _unitClass = transform.Find("class").GetComponent<TMP_Text>();
        _unitPortrait = transform.Find("portrait").GetComponent<Image>();
        _background = transform.Find("background").gameObject;
        _background.SetActive(false);

        _needsWidthUpdate = true;
    }

    public void SetFrozen(AllyCharacter[] toFreeze)
    {
        bool frozen = Array.IndexOf(toFreeze, _character) >= 0;
        _frozen = frozen;
        _background.SetActive(frozen);
    }

    public void SetCharacter(AllyCharacter character)
    {
        _character = character;

        // TODO : name
        _unitName.text = "Name";
        _unitClass.text = character.CharacterClass.ToString();
        _unitPortrait.sprite = character.GetPortrait();
    }

    private void OnMouseEnter()
    {
        if (!_frozen) _background.SetActive(true);

        if (OnMouseEnterEvent != null) OnMouseEnterEvent(_character);
    }

    private void OnMouseExit()
    {
        if (!_frozen) _background.SetActive(false);

        if (OnMouseExitEvent != null) OnMouseExitEvent();
    }

    private void OnMouseUpAsButton()
    {
        if (_frozen) return;

        print("click " + _unitName.text);

        if (OnMouseClickEvent != null) OnMouseClickEvent(_character);
    }

    private void Update()
    {
        if (_needsWidthUpdate)
        {
            RectTransform rect = GetComponent<RectTransform>();
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            box.size = new Vector2(rect.rect.width, box.size.y);

            _needsWidthUpdate = box.size.x < 1f;
        }
    }
}
