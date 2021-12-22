using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class PortraitButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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

    public delegate void RightClickCharacter(AllyCharacter character);
    public static event RightClickCharacter OnCharacterRightClicked;

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

    private AllyUnit GetAllyUnit()
    {
        if (CombatGameManager.Instance.AllAllyUnits.Count <= _buttonOrder)
        {
            return null;
        }

        if (_buttonOrder >= CombatGameManager.Instance.ControllableUnits.Count)
        {
            int index = _buttonOrder - CombatGameManager.Instance.ControllableUnits.Count;
            return CombatGameManager.Instance.AllAllyUnits.Except(CombatGameManager.Instance.ControllableUnits).ElementAt(index);
        }

        return CombatGameManager.Instance.ControllableUnits[_squadIndex];
    }

    private void SelectUnit(int squadIndex)
    {
        if (_buttonOrder >= CombatGameManager.Instance.ControllableUnits.Count) _sprite.color = Color.gray;
        else _sprite.color = Color.white;

        _squadIndex = (squadIndex + _buttonOrder) % CombatGameManager.Instance.ControllableUnits.Count;

        AllyUnit unit = GetAllyUnit();
        if (unit == null) return;
        _sprite.sprite = unit.AllyCharacter.GetPortrait();
        if (_isSelected)
        {
            // TODO : name
            _class.text = unit.AllyCharacter.CharacterClass.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && CombatGameManager.Instance.CurrentAbility == null)
        {
            CombatGameManager.Instance.SelectControllableUnit(GetAllyUnit());
        }
        else if (eventData.button == PointerEventData.InputButton.Right && OnCharacterRightClicked != null)
        {
            print("right");
            OnCharacterRightClicked(GetAllyUnit().AllyCharacter);
        }
    }

    private void Update()
    {
        if (CombatGameManager.Instance.AllAllyUnits.Count <= _buttonOrder)
        {
            gameObject.SetActive(false);
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        CombatGameManager.Instance.Camera.SwitchViewWithoutParenthood(GetAllyUnit());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CombatGameManager.Instance.Camera.SwitchViewBackToParent();
    }
}
