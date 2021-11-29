using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AbilityButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool _duo;
    private BaseAbility _ability;

    public delegate void MouseEnter(BaseAbility ability);
    public static event MouseEnter OnMouseEnter;

    public delegate void MouseExit();
    public static event MouseExit OnMouseExit;

    public void SetAbility(BaseAbility ability)
    {
        _duo = ability is BaseDuoAbility;
        _ability = ability;

        TMP_Text text = GetComponentInChildren<TMP_Text>();
        text.text = ability.GetName();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CombatGameManager.Instance.CurrentUnit.UseAbility(_ability);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (OnMouseEnter != null) OnMouseEnter(_ability);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (OnMouseExit != null) OnMouseExit();
    }
}
