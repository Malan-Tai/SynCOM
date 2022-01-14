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

    public class LinkUnitEntryPart : ColorEntryPart
    {
        public readonly GridBasedUnit TargetUnit;

        public LinkUnitEntryPart(string text, Color color, GridBasedUnit targetUnit) :
            base(text, color)
        {
            TargetUnit = targetUnit;
        }

        public override string ToString()
        {
            return $"<link=\"{_text}\"><color=#{ColorUtility.ToHtmlStringRGBA(_color)}>{_text}</color></link>";
        }
    }
}
