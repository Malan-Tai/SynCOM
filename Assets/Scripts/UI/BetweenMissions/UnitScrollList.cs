using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScrollList : MonoBehaviour
{
    [SerializeField]
    private RectTransform _listElementPrefab;

    [SerializeField]
    private float _offset = 5;

    public delegate void EventMouseEnter(AllyCharacter character);
    public event EventMouseEnter OnMouseEnterEvent;

    public delegate void EventMouseExit();
    public event EventMouseExit OnMouseExitEvent;

    public delegate void EventMouseClick(AllyCharacter character);
    public event EventMouseClick OnMouseClickEvent;

    public void Populate(List<AllyCharacter> characters)
    {
        int i = 0;
        int N = characters.Count;

        RectTransform rect = GetComponent<RectTransform>();
        float sizeDeltaX = rect.sizeDelta.x;
        float height = _listElementPrefab.rect.height + _offset;
        rect.sizeDelta = new Vector2(sizeDeltaX, height * N);

        UnitListElement[] alreadyCreated = GetComponentsInChildren<UnitListElement>();

        foreach (UnitListElement elem in alreadyCreated)
        {
            if (i >= N)
            {
                elem.gameObject.SetActive(false);
                elem.OnMouseEnterEvent  -= ListElementMouseEnter;
                elem.OnMouseExitEvent   -= ListElementMouseExit;
                elem.OnMouseClickEvent  -= ListElementMouseClick;
            }
            else
            {
                elem.gameObject.SetActive(true);
                elem.Init();
                elem.SetCharacter(characters[i]);
            }
            i++;
        }

        while (i < N)
        {
            UnitListElement newElem = Instantiate(_listElementPrefab, transform).GetComponent<UnitListElement>();
            newElem.Init();
            newElem.SetCharacter(characters[i]);
            newElem.transform.localPosition -= new Vector3(0, height * i, 0);

            newElem.OnMouseEnterEvent  += ListElementMouseEnter;
            newElem.OnMouseExitEvent   += ListElementMouseExit;
            newElem.OnMouseClickEvent  += ListElementMouseClick;

            i++;
        }
    }

    public void FreezeCharacters(AllyCharacter[] frozen)
    {
        UnitListElement[] children = GetComponentsInChildren<UnitListElement>();
        foreach (UnitListElement child in children)
        {
            child.SetFrozen(frozen);
        }
    }

    private void ListElementMouseEnter(AllyCharacter character)
    {
        if (OnMouseEnterEvent != null) OnMouseEnterEvent(character);
    }

    private void ListElementMouseExit()
    {
        if (OnMouseExitEvent != null) OnMouseExitEvent();
    }

    private void ListElementMouseClick(AllyCharacter character)
    {
        if (OnMouseClickEvent != null) OnMouseClickEvent(character);
    }
}
