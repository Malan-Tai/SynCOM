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

    [SerializeField] private Color _allyColor;
    [SerializeField] private Color _enemyColor;

    private void Awake()
    {
        _skipFrame = true;
        _sprite = GetComponent<Image>();
        _selectedImage = transform.Find("Image").GetComponent<Image>();
    }

    private void OnEnable()
    {
        BaseAllyAbility.OnTargetSymbolUpdateRequest += SetSelected;
    }

    private void OnDisable()
    {
        BaseAllyAbility.OnTargetSymbolUpdateRequest -= SetSelected;
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
            if (selected is AllyUnit) _selectedImage.color = _allyColor;
            else _selectedImage.color = _enemyColor;
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
