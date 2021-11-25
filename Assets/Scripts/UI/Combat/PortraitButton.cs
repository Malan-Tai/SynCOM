using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PortraitButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private int _buttonOrder;

    [SerializeField]
    private bool _isSelected = false;

    [SerializeField]
    private TMP_Text _name;
    [SerializeField]
    private TMP_Text _class;

    private int _squadIndex;

    private Image _sprite;

    private void Awake()
    {
        _squadIndex = _buttonOrder;
        _sprite = GetComponent<Image>();
    }

    private void OnEnable()
    {
        CombatGameManager.OnUnitSelected += SelectUnit;
    }

    private void OnDisable()
    {
        CombatGameManager.OnUnitSelected -= SelectUnit;
    }

    private void SelectUnit(int squadIndex)
    {
        _squadIndex = (squadIndex + _buttonOrder) % CombatGameManager.Instance.AllAllyUnits.Count;

        // TODO : portrait
        _sprite.sprite = CombatGameManager.Instance.AllAllyUnits[_squadIndex].AllyCharacter.Portrait;

        if (_isSelected)
        {
            // TODO : name
            _class.text = CombatGameManager.Instance.AllAllyUnits[_squadIndex].AllyCharacter.CharacterClass.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            CombatGameManager.Instance.SelectControllableUnit(_squadIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            print("right");
        }
    }

    private void Update()
    {
        if (CombatGameManager.Instance.AllAllyUnits.Count <= _buttonOrder)
        {
            gameObject.SetActive(false);
        }
    }
}
