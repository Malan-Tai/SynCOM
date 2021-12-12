using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSheet : MonoBehaviour
{
    private const float OFFSET_Y = 2000;

    private Image _image;
    private TMP_Text _name;
    private TMP_Text _class;
    private TMP_Text _hpText;
    private TMP_Text _accText;
    private TMP_Text _dmgText;
    private TMP_Text _critText;
    private TMP_Text _evaText;
    private TMP_Text _mvtText;
    private TMP_Text _wgtText;
    private FullRelationshipsPanel _relationships;
    private GameObject _quitButton;

    private Vector3 _basePos;

    private AllyCharacter _clickedCharacter;

    private void Awake()
    {
        _basePos = transform.localPosition;

        _image = transform.Find("Image").GetComponent<Image>();
        _name = transform.Find("Name").GetComponent<TMP_Text>();
        _class = transform.Find("Class").GetComponent<TMP_Text>();

        Transform stats = transform.Find("Stats");
        _hpText = stats.Find("hp").GetComponent<TMP_Text>();
        _accText = stats.Find("acc").GetComponent<TMP_Text>();
        _dmgText = stats.Find("dmg").GetComponent<TMP_Text>();
        _critText = stats.Find("crit").GetComponent<TMP_Text>();
        _evaText = stats.Find("eva").GetComponent<TMP_Text>();
        _mvtText = stats.Find("mvt").GetComponent<TMP_Text>();
        _wgtText = stats.Find("wgt").GetComponent<TMP_Text>();

        _relationships = transform.Find("Relationships").GetComponent<FullRelationshipsPanel>();

        _quitButton = transform.Find("Quit").gameObject;
    }

    public void InitEventsWithScrollList(UnitScrollList list, bool clickableUnits = false)
    {
        list.OnMouseEnterEvent += SetVisible;
        list.OnMouseEnterEvent += SetCharacter;
        if (clickableUnits)
        {
            list.OnMouseClickEvent += ClickCharacter;
            list.OnMouseExitEvent += SetClickedCharacterOrInvisible;
        }
        else
        {
            list.OnMouseExitEvent += SetInvisible;
        }

        this.transform.position += new Vector3(0, OFFSET_Y, 0);

        _quitButton.SetActive(false);
    }

    public void InitEventsFromCombat()
    {
        PortraitButton.OnCharacterRightClicked += SetVisible;
        PortraitButton.OnCharacterRightClicked += SetCharacter;

        this.transform.position += new Vector3(0, OFFSET_Y, 0);
    }

    private void SetVisible(AllyCharacter character)
    {
        //this.transform.position -= new Vector3(0, OFFSET_Y, 0);
        transform.localPosition = _basePos;
    }

    private void SetCharacter(AllyCharacter character)
    {
        // TODO : abilities, name
        _image.sprite = character.GetSprite();
        _class.text = character.CharacterClass.ToString();

        _hpText.text = "HP : " + character.HealthPoints + " / " + character.MaxHealth;
        _accText.text = "Acc : " + character.Accuracy;
        _dmgText.text = "Dmg : " + character.Damage;
        _critText.text = "Crit : " + character.CritChances;
        _evaText.text = character.GetDodge(EnumCover.None) + " : Eva";
        _mvtText.text = character.MovementPoints + " : Mvt";
        _wgtText.text = character.Weigth + " : Wgt";

        _relationships.HoverCharacter(character, false);
    }

    public void SetInvisible()
    {
        this.transform.position += new Vector3(0, OFFSET_Y, 0);
    }

    private void ClickCharacter(AllyCharacter character)
    {
        if (_clickedCharacter == character) _clickedCharacter = null;
        else _clickedCharacter = character;
    }

    private void SetClickedCharacterOrInvisible()
    {
        if (_clickedCharacter != null) SetCharacter(_clickedCharacter);
        else SetInvisible();
    }

    public void ClickRecruitButton()
    {
        if (_clickedCharacter == null) return;

        BetweenMissionsGameManager.Instance.RecruitUnit(_clickedCharacter);
        _clickedCharacter = null;
        SetClickedCharacterOrInvisible();
    }
}
