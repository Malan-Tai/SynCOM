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
        if (isActiveAndEnabled)
        {
            CombatGameManager.Instance.StartCoroutine(UpdateHovering());
        }
    }

    private void OnDisable()
    {
        // Check if scene is stopped otherwise it does lots of useless errors
        if (CombatGameManager.Instance != null)
        {
            // Use CombatGameManager to start coroutine
            CombatGameManager.Instance.StartCoroutine(UpdateHovering());
        }
    }

    private static IEnumerator UpdateHovering()
    {
        yield return null;

        _hoveredElementNumber--;
        if (_hoveredElementNumber < 0) _hoveredElementNumber = 0;
    }
    
    public static void ResetCount()
    {
        _hoveredElementNumber = 0;
    }
}
