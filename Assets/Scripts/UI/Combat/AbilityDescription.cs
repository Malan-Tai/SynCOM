using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AbilityDescription : MonoBehaviour
{
    private const float OFFSET_Y = -1000;

    [SerializeField]
    private EnumAbilityDescription _descriptionType;

    private TMP_Text _title;
    private TMP_Text _description;

    private bool _hidden;

    private Vector3 _basePosition;

    private void Awake()
    {
        _title = transform.Find("Title").GetComponent<TMP_Text>();
        _description = transform.Find("Description").GetComponent<TMP_Text>();

        _basePosition = this.transform.localPosition;
        this.transform.position += new Vector3(0, OFFSET_Y, 0);
        _hidden = true;
    }

    private void OnEnable()
    {
        AllyUnit.OnStartedUsingAbility += StartUsing;
        AllyUnit.OnStoppedUsingAbility += StopUsing;
    }

    private void OnDisable()
    {
        AllyUnit.OnStartedUsingAbility -= StartUsing;
        AllyUnit.OnStoppedUsingAbility -= StopUsing;
    }

    private void StartUsing(BaseAbility ability)
    {
        BaseDuoAbility duo = ability as BaseDuoAbility;
        if ((_descriptionType == EnumAbilityDescription.Solo && duo != null)
            || (_descriptionType != EnumAbilityDescription.Solo && duo == null))
        {
            if (!_hidden)
            {
                this.transform.position += new Vector3(0, OFFSET_Y, 0);
                _hidden = true;
            }
            return;
        }

        if (_hidden)
        {
            //this.transform.position -= new Vector3(0, OFFSET_Y, 0);
            this.transform.localPosition = _basePosition;
            _hidden = false;
        }
        _title.text = ability.GetName();

        switch (_descriptionType)
        {
            case EnumAbilityDescription.Solo:
                _description.text = ability.GetDescription();
                break;
            case EnumAbilityDescription.Self:
                _description.text = duo.GetDescription();
                break;
            case EnumAbilityDescription.Ally:
                _description.text = duo.GetAllyDescription();
                break;
            default:
                break;
        }
    }

    private void StopUsing()
    {
        if (_hidden) return;

        this.transform.position += new Vector3(0, OFFSET_Y, 0);
        _hidden = true;
    }
}
