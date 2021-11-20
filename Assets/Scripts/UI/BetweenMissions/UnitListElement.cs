using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitListElement : MonoBehaviour
{
    private AllyCharacter _character;
    private TMP_Text _unitName;
    private Image _unitPortrait;
    private GameObject _background;

    public delegate void EventMouseEnter(AllyCharacter character);
    public static event EventMouseEnter OnMouseEnterEvent;

    public delegate void EventMouseExit();
    public static event EventMouseExit OnMouseExitEvent;

    public delegate void EventMouseClick(AllyCharacter character);
    public static event EventMouseClick OnMouseClickEvent;

    private bool _needsWidthUpdate = true;

    public void Init()
    {
        _unitName = GetComponentInChildren<TMP_Text>();
        _unitPortrait = transform.Find("portrait").GetComponent<Image>();
        _background = transform.Find("background").gameObject;
        _background.SetActive(false);

        _needsWidthUpdate = true;
    }

    public void SetCharacter(AllyCharacter character)
    {
        _character = character;
        _unitName.text = "Name";
    }

    private void OnMouseEnter()
    {
        _background.SetActive(true);

        if (OnMouseEnterEvent != null) OnMouseEnterEvent(_character);
    }

    private void OnMouseExit()
    {
        _background.SetActive(false);

        if (OnMouseExitEvent != null) OnMouseExitEvent();
    }

    private void OnMouseUpAsButton()
    {
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
