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
    private GameObject _quitButton;

    public delegate void EventMouseEnter(AllyCharacter character);
    public static event EventMouseEnter OnMouseEnterEvent;

    public delegate void EventMouseExit();
    public static event EventMouseExit OnMouseExitEvent;

    public delegate void EventMouseClick(int squadIndex);
    public static event EventMouseClick OnMouseClickEvent;

    private bool _needsWidthUpdate = true;

    public void Init()
    {
        _unitName = transform.Find("Name").GetComponent<TMP_Text>();
        _unitClass = transform.Find("Class").GetComponent<TMP_Text>();
        _unitImage = transform.Find("Image").GetComponent<Image>();
        _background = transform.Find("Background").gameObject;
        _background.SetActive(false);
        _quitButton = transform.Find("Quit").gameObject;

        _needsWidthUpdate = true;
    }

    public void SetIndex(int index)
    {
        _indexInSquad = index;
    }

    public void SetCharacter(AllyCharacter character)
    {
        _character = character;

        if (character == null)
        {
            _unitName.gameObject.SetActive(false);
            _unitClass.gameObject.SetActive(false);
            _unitImage.gameObject.SetActive(false);
            _background.SetActive(true);
            _quitButton.SetActive(false);

            return;
        }

        _unitName.gameObject.SetActive(true);
        _unitClass.gameObject.SetActive(true);
        _unitImage.gameObject.SetActive(true);
        _background.SetActive(false);
        _quitButton.SetActive(true);

        _unitName.text = character.Name.Split(' ')[0];
        _unitClass.text = character.CharacterClass.ToString();
        _unitImage.sprite = character.GetSprite();
    }

    public void SetCharacterToNull()
    {
        SetCharacter(null);
        GlobalGameManager.Instance.SetSquadUnit(_indexInSquad, null);
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
