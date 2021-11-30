using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TargetButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image _sprite;
    private Image _selectedImage;

    private GridBasedUnit _unit;

    private bool _skipFrame;

    private void Awake()
    {
        _skipFrame = true;
        _sprite = GetComponent<Image>();
        _selectedImage = transform.Find("Image").GetComponent<Image>();
    }

    private void OnEnable()
    {
        BaseAbility.OnTargetSymbolUpdateRequest += SetSelected;
    }

    private void OnDisable()
    {
        BaseAbility.OnTargetSymbolUpdateRequest -= SetSelected;
    }

    public void SetUnit(GridBasedUnit unit)
    {
        _unit = unit;
        _sprite.sprite = unit.GetPortrait();

        _skipFrame = true;
    }

    private void SetSelected(GridBasedUnit selected)
    {
        bool isSelected = selected == _unit;
        _selectedImage.gameObject.SetActive(isSelected);
        if (_selectedImage)
        {
            if (selected is AllyUnit) _selectedImage.color = Color.cyan;
            else _selectedImage.color = Color.yellow;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !_skipFrame)
        {
            CombatGameManager.Instance.UIClickTarget(_unit);
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        CombatGameManager.Instance.Camera.SwitchViewWithoutParenthood(_unit);
        CombatGameManager.Instance.AbilityHoverTarget(_unit);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CombatGameManager.Instance.Camera.SwitchViewBackToParent();
        CombatGameManager.Instance.AbilityHoverTarget(null);
    }

    private void Update()
    {
        _skipFrame = false;
    }
}
