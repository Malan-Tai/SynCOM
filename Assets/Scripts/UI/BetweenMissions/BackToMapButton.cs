using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackToMapButton : MonoBehaviour
{
    private const float OFFSET_Y = 200f;

    public delegate void EventMouseClick();
    public static event EventMouseClick OnMouseClickEvent;

    private void Start()
    {
        transform.position -= new Vector3(0, OFFSET_Y, 0);

        GetComponent<Button>().onClick.AddListener(BackToMap);
    }

    private void OnEnable()
    {
        RegionButton.OnMouseClickEvent += ClickRegion;
    }

    private void OnDisable()
    {
        RegionButton.OnMouseClickEvent -= ClickRegion;
    }

    private void BackToMap()
    {
        if (OnMouseClickEvent != null) OnMouseClickEvent();

        transform.position -= new Vector3(0, OFFSET_Y, 0);
    }

    private void ClickRegion(RegionScriptableObject data)
    {
        transform.position += new Vector3(0, OFFSET_Y, 0);
    }
}
