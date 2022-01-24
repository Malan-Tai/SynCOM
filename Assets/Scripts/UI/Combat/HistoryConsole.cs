using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryConsole : MonoBehaviour
{
    public static bool IsVisible { get => _instance._consoleRootGO.activeInHierarchy; }

    [SerializeField] private GameObject _consoleRootGO;
    [SerializeField] private Transform _entriesParent;
    [SerializeField] private TMP_Text _entryTemplate;
    [SerializeField] private ScrollRect _consoleScrollRect;

    private delegate void ExecuteLinkAction();
    private readonly Dictionary<string, ExecuteLinkAction> _linkActions = new Dictionary<string, ExecuteLinkAction>();

    private bool _editingEntry = false;
    private readonly StringBuilder _entryBuilder = new StringBuilder();
    private readonly Stack<ConsoleTag> _tagStack = new Stack<ConsoleTag>();
    private readonly Dictionary<string, LinkTagInfo> _newLinksInfo = new Dictionary<string, LinkTagInfo>();

    protected enum ConsoleTag
    {
        Color,
        Link,
        Icon
    }


    #region Singleton
    public static HistoryConsole Instance { get => _instance; }
    private static HistoryConsole _instance;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(this);
    }
    #endregion


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

    public readonly struct LinkTagInfo
    {
        public readonly GridBasedUnit Unit;
        public readonly Color BaseColor;
        public readonly Color HoverColor;

        public LinkTagInfo(GridBasedUnit unit, Color baseColor, Color hoverColor)
        {
            Unit = unit;
            BaseColor = baseColor;
            HoverColor = hoverColor;
        }
    }

    /// <summary>
    /// Begins a new entry for the console. An entry is a line in the console.
    /// All previous non-ended entries will be disposed of, so make sure you have submitted your previous entries with Submit()
    /// </summary>
    public HistoryConsole BeginEntry()
    {
        _editingEntry = true;
        _entryBuilder.Clear();
        _tagStack.Clear();
        _newLinksInfo.Clear();

        return this;
    }

    public HistoryConsole AddText(string text)
    {
        if (_editingEntry)
        {
            _entryBuilder.Append(text);
        }

        return this;
    }

    /// <summary>
    /// Opens a color tag, which will color the texkt until a new color is defined or until it is closed
    /// </summary>
    public HistoryConsole OpenColorTag(Color color)
    {
        if (_editingEntry)
        {
            _tagStack.Push(ConsoleTag.Color);
            _entryBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>");
        }

        return this;
    }

    /// <summary>
    /// Opens a link tag, which is used to link units in the console so when clicked, the camera can focus on the designed unit.
    /// If a tag is opened with the same link ID as a previous tag, the info of the newly opened tag will replace the old info
    /// </summary>
    /// <param name="linkID">the link ID, has to be unique for each unit</param>
    public HistoryConsole OpenLinkTag(string linkID, GridBasedUnit unit, Color baseColor, Color hoverColor)
    {
        if (!_editingEntry)
        {
            return this;
        }

        _tagStack.Push(ConsoleTag.Link);
        _entryBuilder.Append($"<link=\"{linkID}\"><color=#{ColorUtility.ToHtmlStringRGBA(baseColor)}>");

        LinkTagInfo tagInfo = new LinkTagInfo(unit, baseColor, hoverColor);
        if (_newLinksInfo.ContainsKey(linkID))
        {
            _newLinksInfo[linkID] = tagInfo;
        }
        else
        {
            _newLinksInfo.Add(linkID, tagInfo);
        }

        return this;
    }

    /// <summary>
    /// Opens an icon tag, which will display an inline icon in the console and choose if it is displayed with the same color as the text
    /// </summary>
    /// <param name="iconName">the icon name as specified in the icon atlas defined in your textmeshpro asset</param>
    public HistoryConsole OpenIconTag(string iconName, bool sameColorAsText = false)
    {
        if (_editingEntry)
        {
            string tint = sameColorAsText ? " tint=1" : "";
            _tagStack.Push(ConsoleTag.Icon);
            _entryBuilder.Append($"<sprite name=\"{iconName}\"{tint}>");
        }

        return this;
    }

    /// <summary>
    /// Opens an icon tag, which will display an inline icon in the console
    /// </summary>
    /// <param name="iconName">the icon name as specified in the icon atlas defined in your textmeshpro asset</param>
    /// <param name="color">the color the icon will be colored in</param>
    public HistoryConsole OpenIconTag(string iconName, Color color)
    {
        if (_editingEntry)
        {
            _tagStack.Push(ConsoleTag.Icon);
            _entryBuilder.Append($"<sprite name=\"{iconName}\" color=#{ColorUtility.ToHtmlStringRGBA(color)}>");
        }

        return this;
    }

    /// <summary>
    /// Opens an icon tag, which will display an inline icon in the console and choose if it is displayed with the same color as the text
    /// </summary>
    /// <param name="iconIndex">the icon index as specified in the icon atlas defined in your textmeshpro asset</param>
    public HistoryConsole OpenIconTag(int iconIndex, bool sameColorAsText = false)
    {
        if (_editingEntry)
        {
            string tint = sameColorAsText ? " tint=1" : "";
            _tagStack.Push(ConsoleTag.Icon);
            _entryBuilder.Append($"<sprite index={iconIndex}{tint}>");
        }

        return this;
    }

    /// <summary>
    /// Opens an icon tag, which will display an inline icon in the console
    /// </summary>
    /// <param name="iconIndex">the icon index as specified in the icon atlas defined in your textmeshpro asset</param>
    /// <param name="color">the color the icon will be colored in</param>
    public HistoryConsole OpenIconTag(int iconIndex, Color color)
    {
        if (_editingEntry)
        {
            _tagStack.Push(ConsoleTag.Icon);
            _entryBuilder.Append($"<sprite index={iconIndex} color=#{ColorUtility.ToHtmlStringRGBA(color)}>");
        }

        return this;
    }

    /// <summary>
    /// Closes the latest opened tag, be sure to close all opened tags before submitting an entry or it won't submit
    /// </summary>
    public HistoryConsole CloseTag()
    {
        if (!_editingEntry)
        {
            return this;
        }

        ConsoleTag tag = _tagStack.Pop();
        switch (tag)
        {
            case ConsoleTag.Color:
                _entryBuilder.Append("</color>");
                break;
            case ConsoleTag.Link:
                _entryBuilder.Append("</color></link>");
                break;
            case ConsoleTag.Icon:
                break;
        }

        return this;
    }

    /// <summary>
    /// Closes all opened tags
    /// </summary>
    public HistoryConsole CloseAllOpenedTags()
    {
        if (!_editingEntry)
        {
            return this;
        }

        while (_tagStack.Count != 0)
        {
            CloseTag();
        }

        return this;
    }

    /// <summary>
    /// Send the started entry to the console
    /// </summary>
    public void Submit()
    {
        if (!_editingEntry)
        {
            Debug.LogError("Tentative to submit an entry to history console but no entry was started in the first place.");
            return;
        }

        // Check if all tags have been closed
        if (_tagStack.Count != 0)
        {
            // Close all opened tags
            CloseAllOpenedTags();
        }

        TMP_Text textObject = Instantiate(_entryTemplate, _entriesParent);
        if (_newLinksInfo.Count != 0)
        {
            HistoryLinkedLine line = textObject.gameObject.AddComponent<HistoryLinkedLine>();

            foreach (KeyValuePair<string, LinkTagInfo> linkInfoKVP in _newLinksInfo)
            {
                line.LinksInfo.Add(linkInfoKVP.Key, linkInfoKVP.Value);

                if (!_linkActions.ContainsKey(linkInfoKVP.Key))
                {
                    _linkActions.Add(linkInfoKVP.Key, () => {
                        CombatGameManager.Instance.Camera.SwitchParenthood(linkInfoKVP.Value.Unit);
                    });
                }
            }
        }

        textObject.text = _entryBuilder.ToString();
        Canvas.ForceUpdateCanvases();
        _consoleScrollRect.verticalNormalizedPosition = 0f;

        _entryBuilder.Clear();
        _tagStack.Clear();
        _newLinksInfo.Clear();
        _editingEntry = false;
    }
}
