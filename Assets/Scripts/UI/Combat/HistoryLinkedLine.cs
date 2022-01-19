using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HistoryLinkedLine : MonoBehaviour, IPointerClickHandler
{
    public Color BaseColor;
    public Color HoverColor;

    private TMP_Text _textObject;
    private int _hoveredLinkIndex = -1;

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

    private void Update()
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_textObject, Input.mousePosition, null);

        if (_hoveredLinkIndex != -1 && _hoveredLinkIndex != linkIndex)
        {
            Debug.Log(BaseColor);
            SetLinkToColor(_hoveredLinkIndex, BaseColor);
            _hoveredLinkIndex = -1;
        }

        if (linkIndex != -1 && _hoveredLinkIndex != linkIndex)
        {
            SetLinkToColor(linkIndex, HoverColor);
            _hoveredLinkIndex = linkIndex;
        }
    }

    private void SetLinkToColor(int linkIndex, Color color)
    {
        TMP_LinkInfo linkInfo = _textObject.textInfo.linkInfo[linkIndex];

        for (int i = 0; i < linkInfo.linkTextLength; i++)
        {
            // character index into the entire text
            int characterIndex = linkInfo.linkTextfirstCharacterIndex + i;
            TMP_CharacterInfo charInfo = _textObject.textInfo.characterInfo[characterIndex];
            // material index of this character
            int meshIndex = charInfo.materialReferenceIndex;
            // index of the first vertex of this character
            int vertexIndex = charInfo.vertexIndex;

            // colors of this character
            Color32[] vertexColors = _textObject.textInfo.meshInfo[meshIndex].colors32;

            if (charInfo.isVisible)
            {
                vertexColors[vertexIndex + 0] = color;
                vertexColors[vertexIndex + 1] = color;
                vertexColors[vertexIndex + 2] = color;
                vertexColors[vertexIndex + 3] = color;
            }
        }

        // Update Geometry
        _textObject.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}
