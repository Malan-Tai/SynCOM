using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityScrollList : MonoBehaviour
{
    [SerializeField]
    private RectTransform _listElementPrefab;

    [SerializeField]
    private float _offset = 5;

    public void Populate(List<BaseAllyAbility> abilities)
    {
        int i = 0;
        int N = abilities.Count;

        float currentHeight = 0f;

        AbilityListElement[] alreadyCreated = GetComponentsInChildren<AbilityListElement>();

        foreach (AbilityListElement elem in alreadyCreated)
        {
            if (i >= N)
            {
                elem.gameObject.SetActive(false);
            }
            else
            {
                elem.gameObject.SetActive(true);
                elem.Init();
                float height = elem.SetAbility(abilities[i]) + _offset;
                elem.transform.localPosition = new Vector3(15, -currentHeight - 15, 0);
                currentHeight += height;
            }
            i++;
        }

        while (i < N)
        {
            AbilityListElement newElem = Instantiate(_listElementPrefab, transform).GetComponent<AbilityListElement>();
            newElem.Init();
            float height = newElem.SetAbility(abilities[i]) + _offset;
            newElem.transform.localPosition = new Vector3(15, -currentHeight - 15, 0);
            currentHeight += height;

            i++;
        }

        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(_listElementPrefab.sizeDelta.x - 7, currentHeight);
    }
}
