using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TargetButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image _sprite;

    private GridBasedUnit _unit;

    private bool _skipFrame;

    private void Awake()
    {
        _skipFrame = true;
        _sprite = GetComponent<Image>();
    }

    public void SetUnit(GridBasedUnit unit)
    {
        _unit = unit;
        _sprite.sprite = unit.GetPortrait();
        _skipFrame = true;
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
