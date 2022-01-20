using UnityEngine;

namespace EntryParts
{
    public class EntryPart
    {
        protected string _text;

        public EntryPart(string text)
        {
            _text = text;
        }

        public override string ToString()
        {
            return _text.Trim();
        }
    }

    public class ColorEntryPart : EntryPart
    {
        protected Color _color;

        public ColorEntryPart(string text, Color color) :
            base(text)
        {
            _color = color;
        }

        public override string ToString()
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(_color)}>{_text}</color>";
        }
    }

    public class IconEntryPart : ColorEntryPart
    {
        protected int _index;
        protected bool _useIndexRatherThanName;

        public IconEntryPart(string text, Color color) :
            base(text, color)
        {
            _useIndexRatherThanName = false;
            _index = 0;
        }

        public IconEntryPart(int index, Color color) :
            base("", color)
        {
            _useIndexRatherThanName = true;
            _index = index;
        }

        public override string ToString()
        {
            if (_useIndexRatherThanName)
            {
                return $"<sprite index={_index} color=#{ColorUtility.ToHtmlStringRGBA(_color)}>";
            }

            return $"<sprite name=\"{_text}\" color=#{ColorUtility.ToHtmlStringRGBA(_color)}>";
        }
    }

    public class LinkUnitEntryPart : ColorEntryPart
    {
        public readonly GridBasedUnit TargetUnit;
        public readonly Color HoverColor;
        public Color Color { get => _color; }

        public LinkUnitEntryPart(string text, GridBasedUnit targetUnit, Color color, Color hoverColor) :
            base(text, color)
        {
            TargetUnit = targetUnit;
            HoverColor = hoverColor;
        }

        public override string ToString()
        {
            return $"<link=\"{_text}\"><color=#{ColorUtility.ToHtmlStringRGBA(_color)}>{_text}</color></link>";
        }
    }
}
