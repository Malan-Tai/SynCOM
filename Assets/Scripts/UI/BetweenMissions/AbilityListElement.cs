using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AbilityListElement : MonoBehaviour
{
    private TMP_Text _name;
    private TMP_Text _description;

    public void Init()
    {
        _name = transform.Find("Name").GetComponent<TMP_Text>();
        _description = transform.Find("Description").GetComponent<TMP_Text>();
    }

    public float SetAbility(BaseAllyAbility ability)
    {
        _name.text = ability.GetName();
        _description.text = ability.GetShortDescription();

        _name.ForceMeshUpdate();
        _description.ForceMeshUpdate();

        return _name.mesh.bounds.size.y + _description.mesh.bounds.size.y;
    }
}
