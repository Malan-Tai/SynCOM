using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoCanvas : MonoBehaviour
{
    [SerializeField] private Sprite _fullShield;
    [SerializeField] private Sprite _halfShield;
    [SerializeField] private Sprite _emptyShield;

    [SerializeField] private Color _enemyBarColor;

    private Slider _hp;
    private TMP_Text _hpText;
    private Image _cover;

    private bool _forced;

    private void Awake()
    {
        _hp = GetComponentInChildren<Slider>();
        _hpText = _hp.transform.GetComponentInChildren<TMP_Text>();
        _cover = transform.Find("Cover").GetComponent<Image>();

        _hpText.enabled = false;

        _forced = false;
    }

    public void HideCover()
    {
        _cover.enabled = false;
    }

    public void SetCover(EnumCover cover)
    {
        _cover.enabled = true;
        switch (cover)
        {
            case EnumCover.None:
                _cover.sprite = _emptyShield;
                break;
            case EnumCover.Half:
                _cover.sprite = _halfShield;
                break;
            case EnumCover.Full:
                _cover.sprite = _fullShield;
                break;
            default:
                break;
        }
    }

    public void SetHP(float hp, float maxHP)
    {
        _hp.SetValueWithoutNotify(hp / maxHP);
        _hpText.SetText((int)hp + "/" + (int)maxHP);
    }

    public void SetBig(bool force)
    {
        _forced = _forced || force;

        var rect = _hp.transform as RectTransform;
        rect.sizeDelta = new Vector2(450, 100);

        _hpText.enabled = true;
    }

    public void SetSmall(bool force)
    {
        if (_forced && !force) return;
        _forced = false; // _forced && !force;

        var rect = _hp.transform as RectTransform;
        rect.sizeDelta = new Vector2(150, 50);

        _hpText.enabled = false;
    }

    public void SetEnemy()
    {
        _hp.transform.Find("Fill Area").GetChild(0).GetComponent<Image>().color = _enemyBarColor;
    }
}
