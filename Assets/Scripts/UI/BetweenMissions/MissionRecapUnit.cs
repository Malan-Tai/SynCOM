using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionRecapUnit : MonoBehaviour
{
    private int _indexInSquad;
    private AllyCharacter _character;
    private TMP_Text _unitName;
    private TMP_Text _unitClass;
    private Image _unitImage;
    private GameObject _background;

    public delegate void EventMouseEnter(AllyCharacter character);
    public static event EventMouseEnter OnMouseEnterEvent;

    public delegate void EventMouseExit();
    public static event EventMouseExit OnMouseExitEvent;

    public delegate void EventMouseClick(int squadIndex);
    public static event EventMouseClick OnMouseClickEvent;

    private bool _needsWidthUpdate = true;

    private void Start()
    {
        _unitName = transform.Find("Name").GetComponent<TMP_Text>();
        _unitClass = transform.Find("Class").GetComponent<TMP_Text>();
        _unitImage = transform.Find("Image").GetComponent<Image>();
        _background = transform.Find("Background").gameObject;
        _background.SetActive(false);

        _needsWidthUpdate = true;
    }

    public void SetCharacter(int index, AllyCharacter character)
    {
        _indexInSquad = index;
        _character = character;

        if (character == null)
        {
            _unitName.gameObject.SetActive(false);
            _unitClass.gameObject.SetActive(false);
            _unitImage.gameObject.SetActive(false);
            _background.SetActive(true);

            return;
        }

        _unitName.gameObject.SetActive(true);
        _unitClass.gameObject.SetActive(true);
        _unitImage.gameObject.SetActive(true);
        _background.SetActive(false);

        // TODO : portrait & name
        _unitClass.text = character.CharacterClass.ToString();
    }

    private void OnMouseEnter()
    {
        _background.SetActive(true);

        if (OnMouseEnterEvent != null) OnMouseEnterEvent(_character);
    }

    private void OnMouseExit()
    {
        _background.SetActive(_character == null);

        if (OnMouseExitEvent != null) OnMouseExitEvent();
    }

    private void OnMouseUpAsButton()
    {
        print("click " + _indexInSquad + " : " + _unitName.text);

        if (OnMouseClickEvent != null) OnMouseClickEvent(_indexInSquad);
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
