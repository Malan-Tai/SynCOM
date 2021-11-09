using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DistrictButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private Color _hoverColor;

    private Image _sprite;

    private void Start()
    {
        _sprite = GetComponent<Image>();
        _sprite.alphaHitTestMinimumThreshold = 0.1f;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        //print("enter " + _name);
        _sprite.color = _hoverColor;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        //print("exit " + _name);
        _sprite.color = Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("clicked " + _name);
    }
}
