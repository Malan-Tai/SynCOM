using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TraitListElement : MonoBehaviour
{
    private TMP_Text _name;
    private TMP_Text _description;

    public void Init()
    {
        _name = transform.Find("Name").GetComponent<TMP_Text>();
        _description = transform.Find("Description").GetComponent<TMP_Text>();
    }

    public float SetTrait(Trait trait)
    {
        _name.text = trait.GetName();
        _description.text = trait.GetDescription();

        _name.ForceMeshUpdate();
        _description.ForceMeshUpdate();

        return _name.mesh.bounds.size.y + _description.mesh.bounds.size.y;
    }
}
