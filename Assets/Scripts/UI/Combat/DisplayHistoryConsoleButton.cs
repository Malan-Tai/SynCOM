using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayHistoryConsoleButton : MonoBehaviour
{
    [SerializeField] private bool _startHidden = true;


    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(ToggleHistoryConsole);

        if (_startHidden)
        {
            HistoryConsole.Display(false);
        }
        else
        {
            HistoryConsole.Display(true);
        }
    }

    public void ToggleHistoryConsole()
    {
        if (HistoryConsole.IsVisible)
        {
            HistoryConsole.Display(false);
        }
        else
        {
            HistoryConsole.Display(true);
        }

        EventSystem.current.SetSelectedGameObject(null);
    }
}
