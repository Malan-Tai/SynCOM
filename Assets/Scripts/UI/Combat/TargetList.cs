using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetList : MonoBehaviour
{
    private const float OFFSET_Y = -200f;

    [SerializeField]
    private RectTransform _targetBtnPrefab;

    private RectTransform _rectTransform;

    private Vector3 _basePosition;

    private void Awake()
    {
        _basePosition = transform.position;

        _rectTransform = GetComponent<RectTransform>();

        Hide();
    }

    private void OnEnable()
    {
        BaseAllyAbility.OnTargetsUpdateRequest += Show;
        AllyUnit.OnStoppedUsingAbility += Hide;
    }

    private void OnDisable()
    {
        BaseAllyAbility.OnTargetsUpdateRequest -= Show;
        AllyUnit.OnStoppedUsingAbility -= Hide;
    }

    public void Populate(IEnumerable<GridBasedUnit> units)
    {
        float x = 10;
        int i = 0;
        TargetButton[] _buttons = GetComponentsInChildren<TargetButton>();

        _rectTransform.sizeDelta = new Vector2(10, 100);

        foreach (GridBasedUnit unit in units)
        {
            TargetButton btn;
            if (i < _buttons.Length)
            {
                btn = _buttons[i];
                btn.SetUnit(unit);
            }
            else
            {
                btn = Instantiate(_targetBtnPrefab, transform).GetComponent<TargetButton>();
                btn.transform.localPosition += new Vector3(x, -10, 0);
            }

            float width = btn.GetComponent<RectTransform>().rect.width + 10;
            _rectTransform.sizeDelta += new Vector2(width, 0);
            x += width;
            btn.SetUnit(unit);
            i++;
        }

        while (i < _buttons.Length)
        {
            //float width = _buttons[i].GetComponent<RectTransform>().rect.width + 10;
            //_rectTransform.sizeDelta -= new Vector2(width, 0);

            Destroy(_buttons[i].gameObject);

            i++;
        }
    }

    private void Show(IEnumerable<GridBasedUnit> units)
    {
        this.transform.position = _basePosition;
        Populate(units);
    }

    private void Hide()
    {
        this.transform.localPosition += new Vector3(0, OFFSET_Y, 0);
    }
}
