using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoCanvas : MonoBehaviour
{
    [SerializeField] private Sprite _fullShield;
    [SerializeField] private Sprite _halfShield;
    [SerializeField] private Sprite _emptyShield;

    private Slider _hp;
    private Image _cover;

    private void Awake()
    {
        _hp = GetComponentInChildren<Slider>();
        _cover = transform.Find("Cover").GetComponent<Image>();
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

    public void SetRatioHP(float ratio)
    {
        _hp.SetValueWithoutNotify(ratio);
    }
}
