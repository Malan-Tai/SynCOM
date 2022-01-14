using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EntryParts;

public class HistoryConsole : MonoBehaviour
{
    public static readonly Color UNIT_LINK_COLOR = Color.red;

    [SerializeField] private Transform _entriesParent;
    [SerializeField] private TMP_Text _entryTemplate;

    private ScrollRect _consoleScrollRect;
    private Image _consoleBackground;

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


    private void Start()
    {
        _consoleBackground = GetComponent<Image>();
        _consoleScrollRect = GetComponent<ScrollRect>();
        _consoleBackground.enabled = _entriesParent.childCount > 0;
    }

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
                    textObject.gameObject.AddComponent<HistoryLinkedLine>();
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

        if (!_instance._consoleBackground.enabled)
        {
            _instance._consoleBackground.enabled = true;
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
