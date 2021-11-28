using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AbilityButton : MonoBehaviour, IPointerClickHandler
{
    private bool _duo;
    private BaseAbility _ability;

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
}
