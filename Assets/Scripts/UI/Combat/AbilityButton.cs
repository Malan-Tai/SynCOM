using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AbilityButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool _duo;
    private BaseAllyAbility _ability;

    public delegate void MouseEnter(BaseAllyAbility ability);
    public static event MouseEnter OnMouseEnter;

    public delegate void MouseExit();
    public static event MouseExit OnMouseExit;

    public void SetAbility(BaseAllyAbility ability)
    {
        _duo = ability is BaseDuoAbility;
        _ability = ability;

        TMP_Text text = GetComponentInChildren<TMP_Text>();
        text.text = ability.GetName();

        Transform duoImage = transform.Find("duo");
        duoImage.gameObject.SetActive(_duo);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CombatGameManager.Instance.CurrentUnit.UseAbility(_ability);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (OnMouseEnter != null && CombatGameManager.Instance.CurrentAbility == null) OnMouseEnter(_ability);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (OnMouseExit != null && CombatGameManager.Instance.CurrentAbility == null) OnMouseExit();
    }
}
