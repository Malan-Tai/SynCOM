using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RegionDescription : MonoBehaviour
{
    private TMP_Text _regionName;
    private TMP_Text _regionDescription;

    private bool _eraseOnExit;
    private bool _setOnEnter;

    private void Start()
    {
        _regionName         = this.transform.Find("RegionName").GetComponent<TMP_Text>();
        _regionDescription  = this.transform.Find("RegionDescription").GetComponent<TMP_Text>();

        _eraseOnExit = true;
        _setOnEnter = true;
    }

    private void OnEnable()
    {
        RegionButton.OnMouseExitEvent += EraseDescription;
        RegionButton.OnMouseEnterEvent += SetDescription;
        RegionButton.OnMouseClickEvent += FreezeDescription;
    }

    private void OnDisable()
    {
        RegionButton.OnMouseExitEvent -= EraseDescription;
        RegionButton.OnMouseEnterEvent -= SetDescription;
        RegionButton.OnMouseClickEvent -= FreezeDescription;
    }

    public void SetDescription(RegionScriptableObject data)
    {
        if (!_setOnEnter) return;

        _regionName.text = data.regionName.ToString();
        _regionDescription.text = data.description;
    }

    public void EraseDescription()
    {
        if (!_eraseOnExit) return;

        _regionName.text = "";
        _regionDescription.text = "";
    }

    public void FreezeDescription()
    {
        _eraseOnExit = false;
        _setOnEnter = false;
    }
}
