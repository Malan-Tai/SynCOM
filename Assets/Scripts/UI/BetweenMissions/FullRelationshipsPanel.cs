using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullRelationshipsPanel : MonoBehaviour
{
    private RelationshipPanel[] _panels;

    void Start()
    {
        _panels = GetComponentsInChildren<RelationshipPanel>();

        foreach (RelationshipPanel panel in _panels)
        {
            panel.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        MissionRecapUnit.OnMouseEnterEvent += HoverCharacter;
        MissionRecapUnit.OnMouseExitEvent += ClearPanel;
    }

    private void OnDisable()
    {
        MissionRecapUnit.OnMouseEnterEvent -= HoverCharacter;
        MissionRecapUnit.OnMouseExitEvent -= ClearPanel;
    }

    public void HoverCharacter(AllyCharacter character)
    {
        HoverCharacter(character, true);
    }

    public void HoverCharacter(AllyCharacter character, bool selfIsLeft)
    {
        if (character == null || _panels == null) return;

        int i = 0;
        foreach (AllyCharacter ally in GlobalGameManager.Instance.currentSquad)
        {
            if (ally == character || ally == null || i >= _panels.Length) continue;

            _panels[i].gameObject.SetActive(true);
            if (selfIsLeft) _panels[i].SetPanel(character, ally);
            else _panels[i].SetPanel(ally, character);
            i++;
        }
    }

    private void ClearPanel()
    {
        foreach (RelationshipPanel panel in _panels)
        {
            panel.gameObject.SetActive(false);
        }
    }
}
