using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitScrollList : MonoBehaviour
{
    [SerializeField]
    private RectTransform _listElementPrefab;

    [SerializeField]
    private float _offset = 5;

    public void Populate(List<Trait> traits)
    {
        int i = 0;
        int N = traits.Count;

        float currentHeight = 0f;

        TraitListElement[] alreadyCreated = GetComponentsInChildren<TraitListElement>();

        foreach (TraitListElement elem in alreadyCreated)
        {
            if (i >= N)
            {
                elem.gameObject.SetActive(false);
            }
            else
            {
                elem.gameObject.SetActive(true);
                elem.Init();
                float height = elem.SetTrait(traits[i]) + _offset;
                elem.transform.localPosition = new Vector3(0, -currentHeight, 0);
                currentHeight += height;
            }
            i++;
        }

        while (i < N)
        {
            TraitListElement newElem = Instantiate(_listElementPrefab, transform).GetComponent<TraitListElement>();
            newElem.Init();
            float height = newElem.SetTrait(traits[i]) + _offset;
            print(currentHeight);
            newElem.transform.localPosition = new Vector3(0, -currentHeight, 0);
            currentHeight += height;

            i++;
        }

        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, currentHeight);
    }
}
