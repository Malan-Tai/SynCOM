using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayHistoryConsoleButton : MonoBehaviour
{
    [SerializeField] private bool _startHidden = true;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _shownIconSprite;
    [SerializeField] private Sprite _hiddenIconSprite;


    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(ToggleHistoryConsole);

        if (_startHidden)
        {
            HistoryConsole.Display(false);
            _iconImage.sprite = _hiddenIconSprite;
        }
        else
        {
            HistoryConsole.Display(true);
            _iconImage.sprite = _shownIconSprite;
        }
    }

    public void ToggleHistoryConsole()
    {
        if (HistoryConsole.IsVisible)
        {
            HistoryConsole.Display(false);
            _iconImage.sprite = _hiddenIconSprite;
        }
        else
        {
            HistoryConsole.Display(true);
            _iconImage.sprite = _shownIconSprite;
        }

        EventSystem.current.SetSelectedGameObject(null);
    }
}
