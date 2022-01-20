using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EntryParts;

public class HistoryConsole : MonoBehaviour
{
    public static bool IsVisible { get => _instance._consoleRootGO.activeInHierarchy; }

    [SerializeField] private GameObject _consoleRootGO;
    [SerializeField] private Transform _entriesParent;
    [SerializeField] private TMP_Text _entryTemplate;
    [SerializeField] private ScrollRect _consoleScrollRect;

    private delegate void ExecuteLinkAction();
    private readonly Dictionary<string, ExecuteLinkAction> _linkActions = new Dictionary<string, ExecuteLinkAction>();


    #region Singleton
    private static HistoryConsole _instance;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(this);
    }
    #endregion


    public static void AddEntry(List<EntryPart> entry)
    {
        AddEntry(entry.ToArray());
    }

    public static void AddEntry(EntryPart[] entry)
    {
        TMP_Text textObject = Instantiate(_instance._entryTemplate, _instance._entriesParent);
        StringBuilder stringBuilder = new StringBuilder();
        bool isLinked = false;

        foreach (EntryPart entryPart in entry)
        {
            stringBuilder.Append(entryPart.ToString()).Append(" ");

            if (entryPart is LinkUnitEntryPart luep)
            {
                // Add HistoryLinkedLine component only once per entry
                if (!isLinked)
                {
                    HistoryLinkedLine line = textObject.gameObject.AddComponent<HistoryLinkedLine>();
                    line.HoverColor = luep.HoverColor;
                    line.BaseColor = luep.Color;
                    isLinked = true;
                }

                if (!_instance._linkActions.ContainsKey(luep.TargetUnit.Character.Name))
                {
                    _instance._linkActions.Add(luep.TargetUnit.Character.Name, () => {
                        CombatGameManager.Instance.Camera.SwitchParenthood(luep.TargetUnit);
                    });
                }
            }
        }

        textObject.text = stringBuilder.ToString().Trim();
        Canvas.ForceUpdateCanvases();
        _instance._consoleScrollRect.verticalNormalizedPosition = 0f;
    }

    public static void Display(bool display)
    {
        _instance._consoleRootGO.SetActive(display);

        if (display)
        {
            _instance._consoleScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public static void ExecuteLink(string linkName)
    {
        if (!_instance._linkActions.ContainsKey(linkName))
        {
            return;
        }

        _instance._linkActions[linkName]();
    }
}
