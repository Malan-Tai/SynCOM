using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScrollList : MonoBehaviour
{
    [SerializeField]
    private RectTransform _listElementPrefab;

    [SerializeField]
    private float _offset = 5;

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

            i++;
        }
    }
}
