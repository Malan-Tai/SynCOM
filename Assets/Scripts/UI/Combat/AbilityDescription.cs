using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityDescription : MonoBehaviour
{
    private const float OFFSET_Y = -1000;

    [SerializeField]
    private EnumAbilityDescription _descriptionType;

    [SerializeField]
    [Tooltip("leav empty if not left or right duo description")]
    private Sprite[] _emojis;

    private TMP_Text _title;
    private TMP_Text _description;

    private Image _portrait;
    private Image _emoji;

    private bool _hidden;

    private Vector3 _basePosition;

    private void Awake()
    {
        switch (_descriptionType)
        {
            case EnumAbilityDescription.Solo:
                _title = transform.Find("Title").GetComponent<TMP_Text>();
                _description = transform.Find("Description").GetComponent<TMP_Text>();
                break;

            case EnumAbilityDescription.Duo:
                _title = transform.Find("Title").GetComponent<TMP_Text>();
                break;

            default: // ally or self
                _portrait = transform.Find("Portrait").GetComponent<Image>();
                _description = transform.Find("Description").GetComponent<TMP_Text>();
                _emoji = transform.Find("Emoji").GetComponent<Image>();
                break;

        }

        _basePosition = this.transform.localPosition;
        this.transform.position += new Vector3(0, OFFSET_Y, 0);
        _hidden = true;
    }

    private void OnEnable()
    {
        AllyUnit.OnStartedUsingAbility += StartUsing;
        AllyUnit.OnStoppedUsingAbility += StopUsing;

        AbilityButton.OnMouseEnter += StartUsing;
        AbilityButton.OnMouseExit += StopUsing;

        BaseAllyAbility.OnDescriptionUpdateRequest += UpdateDescription;
    }

    private void OnDisable()
    {
        AllyUnit.OnStartedUsingAbility -= StartUsing;
        AllyUnit.OnStoppedUsingAbility -= StopUsing;

        AbilityButton.OnMouseEnter -= StartUsing;
        AbilityButton.OnMouseExit -= StopUsing;

        BaseAllyAbility.OnDescriptionUpdateRequest -= UpdateDescription;
    }

    private void StartUsing(BaseAllyAbility ability)
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
            this.transform.localPosition = _basePosition;
            _hidden = false;
        }

        switch (_descriptionType)
        {
            case EnumAbilityDescription.Solo:
                _title.text = ability.GetName();
                _description.text = ability.GetDescription();
                break;

            case EnumAbilityDescription.Duo:
                _title.text = ability.GetName();
                break;

            case EnumAbilityDescription.Self:
                _description.text = duo.GetDescription();

                Sprite portrait = duo.GetSelfPortrait();
                if (portrait == null)
                {
                    _portrait.gameObject.SetActive(false);
                    _emoji.gameObject.SetActive(false);
                }
                else
                {
                    _portrait.gameObject.SetActive(true);
                    _portrait.sprite = portrait;

                    int relationshipStatus = duo.GetRelationshipStatus(true);
                    if (relationshipStatus <= -2) _emoji.gameObject.SetActive(false);
                    else
                    {
                        _emoji.gameObject.SetActive(true);
                        _emoji.sprite = _emojis[1 + relationshipStatus];
                    }
                }

                break;

            case EnumAbilityDescription.Ally:
                _description.text = duo.GetAllyDescription();

                portrait = duo.GetAllyPortrait();
                if (portrait == null)
                {
                    _portrait.gameObject.SetActive(false);
                    _emoji.gameObject.SetActive(false);
                }
                else
                {
                    _portrait.gameObject.SetActive(true);
                    _portrait.sprite = portrait;

                    int relationshipStatus = duo.GetRelationshipStatus(false);
                    if (relationshipStatus <= -2) _emoji.gameObject.SetActive(false);
                    else
                    {
                        _emoji.gameObject.SetActive(true);
                        _emoji.sprite = _emojis[1 + relationshipStatus];
                    }
                }

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

    private void UpdateDescription(BaseAllyAbility ability)
    {
        if (_hidden) return;

        BaseDuoAbility duo = ability as BaseDuoAbility;
        switch (_descriptionType)
        {
            case EnumAbilityDescription.Solo:
                _description.text = ability.GetDescription();
                break;
            case EnumAbilityDescription.Self:
                _description.text = duo.GetDescription();

                Sprite portrait = duo.GetSelfPortrait();
                if (portrait == null)
                {
                    _portrait.gameObject.SetActive(false);
                    _emoji.gameObject.SetActive(false);
                }
                else
                {
                    _portrait.gameObject.SetActive(true);
                    _portrait.sprite = portrait;

                    int relationshipStatus = duo.GetRelationshipStatus(true);
                    if (relationshipStatus <= -2) _emoji.gameObject.SetActive(false);
                    else
                    {
                        _emoji.gameObject.SetActive(true);
                        _emoji.sprite = _emojis[1 + relationshipStatus];
                    }
                }

                break;
            case EnumAbilityDescription.Ally:
                _description.text = duo.GetAllyDescription();

                portrait = duo.GetAllyPortrait();
                if (portrait == null)
                {
                    _portrait.gameObject.SetActive(false);
                    _emoji.gameObject.SetActive(false);
                }
                else
                {
                    _portrait.gameObject.SetActive(true);
                    _portrait.sprite = portrait;

                    int relationshipStatus = duo.GetRelationshipStatus(false);
                    if (relationshipStatus <= -2) _emoji.gameObject.SetActive(false);
                    else
                    {
                        _emoji.gameObject.SetActive(true);
                        _emoji.sprite = _emojis[1 + relationshipStatus];
                    }
                }

                break;
            default:
                break;
        }
    }
}
