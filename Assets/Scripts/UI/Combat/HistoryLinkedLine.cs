using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HistoryLinkedLine : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text _textObject;

    private void Start()
    {
        _textObject = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_textObject, eventData.position, null);
        if (linkIndex != -1)
        { // execute the corresponding action in HistoryConsole when a link is clicked
            TMP_LinkInfo linkInfo = _textObject.textInfo.linkInfo[linkIndex];
            HistoryConsole.ExecuteLink(linkInfo.GetLinkID());
        }
    }
}
