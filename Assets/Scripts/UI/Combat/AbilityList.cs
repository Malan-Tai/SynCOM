using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityList : MonoBehaviour
{
    private const float OFFSET_Y = -200f;

    [SerializeField]
    private RectTransform _abilityBtnPrefab;
    [SerializeField]
    private float _height = 100;

    private RectTransform _rectTransform;

    private Vector3 _basePosition;

    private void Awake()
    {
        _basePosition = transform.position;

        _rectTransform = GetComponent<RectTransform>();

        Populate(new List<BaseAllyAbility>
        {
            new HunkerDown(),
            new BasicShot(),
            new BasicDuoShot(),
            new FirstAid(),
            new Slap(),
            new Devouring()
        });

        Hide(null);
    }

    private void OnEnable()
    {
        AllyUnit.OnStartedUsingAbility += Hide;
        AllyUnit.OnStoppedUsingAbility += Show;
        CombatGameManager.OnUnitSelected += PopulateFromUnit;
        Objective.OnScalingDone += Show;
    }

    private void OnDisable()
    {
        AllyUnit.OnStartedUsingAbility -= Hide;
        AllyUnit.OnStoppedUsingAbility -= Show;
        CombatGameManager.OnUnitSelected -= PopulateFromUnit;
        Objective.OnScalingDone -= Show;
    }

    public void Populate(List<BaseAllyAbility> abilities)
    {
        float x = 10;
        int i = 0;
        AbilityButton[] _buttons = GetComponentsInChildren<AbilityButton>();

        _rectTransform.sizeDelta = new Vector2(10, _height);
        
        foreach (BaseAllyAbility ability in abilities)
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
                btn.transform.localPosition += new Vector3(x, -10, 0);
            }

            float width = btn.GetComponent<RectTransform>().rect.width + 10;
            _rectTransform.sizeDelta += new Vector2(width, 0);
            x += width;
            btn.SetAbility(ability);
            i++;
        }

        while (i < _buttons.Length)
        {
            //float width = _buttons[i].GetComponent<RectTransform>().rect.width + 10;
            //_rectTransform.sizeDelta -= new Vector2(width, 0);

            Destroy(_buttons[i].gameObject);

            i++;
        }
    }

    private void Hide(BaseAllyAbility ability)
    {
        this.transform.localPosition += new Vector3(0, OFFSET_Y, 0);
    }

    private void Show()
    {
        this.transform.position = _basePosition;
    }

    private void PopulateFromUnit(int squadIndex)
    {
        Populate(CombatGameManager.Instance.ControllableUnits[squadIndex].AllyCharacter.Abilities);
    }
}
