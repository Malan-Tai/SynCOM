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
        float x = 10;
        int i = 0;
        AbilityButton[] _buttons = GetComponentsInChildren<AbilityButton>();

        _rectTransform.sizeDelta = new Vector2(10, 100);
        
        foreach (BaseAbility ability in abilities)
        {
            AbilityButton btn;
            if (i < _buttons.Length)
            {
                btn = _buttons[i];
                btn.SetAbility(ability);
            }
            else
            {
                btn = Instantiate(_abilityBtnPrefab, transform).GetComponent<AbilityButton>();
                btn.transform.localPosition += new Vector3(x, 0, 0);
            }

            float width = btn.GetComponent<RectTransform>().rect.width + 10;
            _rectTransform.sizeDelta += new Vector2(width, 0);
            x += width;
            btn.SetAbility(ability);
            i++;
        }

        while (i < _buttons.Length)
        {
            float width = _buttons[i].GetComponent<RectTransform>().rect.width + 10;
            _rectTransform.sizeDelta -= new Vector2(width, 0);

            Destroy(_buttons[i].gameObject);

            i++;
        }
    }
}
