using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockingUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsUIHovered { get => _hoveredElementNumber > 0; }
    private static int _hoveredElementNumber = 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoveredElementNumber++;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hoveredElementNumber--;
        if (_hoveredElementNumber < 0) _hoveredElementNumber = 0;
    }

    private void OnDisable()
    {
        _hoveredElementNumber--;
        if (_hoveredElementNumber < 0) _hoveredElementNumber = 0;
    }
}
