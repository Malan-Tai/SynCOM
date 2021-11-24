using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityList : MonoBehaviour
{
    [SerializeField]
    private RectTransform _abilityBtnPrefab;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        Populate(new List<BaseAbility>
        {
            new HunkerDown(),
            new BasicShot(),
            new BasicDuoShot(),
            new FirstAid(),
            new Slap(),
            new Devouring()
        });
    }

    public void Populate(List<BaseAbility> abilities)
    {
        float x = 0;
        foreach (BaseAbility ability in abilities)
        {
            AbilityButton btn = Instantiate(_abilityBtnPrefab, transform).GetComponent<AbilityButton>();
            btn.transform.position += new Vector3(x, 0, 0);
            float width = btn.GetComponent<RectTransform>().rect.width + 10;
            _rectTransform.sizeDelta += new Vector2(width, 0);
            x += width;

            btn.SetAbility(ability);
        }
    }
}
