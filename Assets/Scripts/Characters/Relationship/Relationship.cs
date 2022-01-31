using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class Relationship
{
    /// <summary>
    /// The <c>Character</c> towards which the sentiments and emotions are felt.
    /// </summary>
    private AllyCharacter _target;
    public AllyCharacter Target
    {
        get { return _target; }
        private set { _target = value; }
    }

    private AllyCharacter _source;

    [Serializable]
    private class Gauge
    {
        /// <summary>
        /// The level of the gauge increase or decrease if the value
        /// becomes high or low enough. The levels of each gauge determines the emotions.
        /// </summary>
        public int level;
        /// <summary>
        /// The value increases or decreases depending on the Duo Abilities
        /// performed with the target.
        /// </summary>
        public int value;

        public Gauge()
        {
            this.level = 0;
            this.value = 0;
        }
    }

    private Dictionary<EnumSentiment, Gauge> _gauges;
    private Gauge _admirationGauge { get { return _gauges[EnumSentiment.Admiration]; } }
    private Gauge _trustGauge { get { return _gauges[EnumSentiment.Trust]; } }
    private Gauge _sympathyGauge { get { return _gauges[EnumSentiment.Sympathy]; } }

    private List<EnumEmotions> _listEmotions;
    public List<EnumEmotions> ListEmotions { get { return _listEmotions; } }

    public bool CheckedDuoRefusal { get; set; } = false;
    public bool AcceptedDuo { get; set; }

    /// <summary>
    /// Returns the current value of the gauge representing the <c>sentiment</c>.
    /// </summary>
    public int GetGaugeValue(EnumSentiment sentiment)
    {
        return _gauges[sentiment].value;
    }

    /// <summary>
    /// Returns the current level of the gauge representing the <c>sentiment</c>.
    /// </summary>
    public int GetGaugeLevel(EnumSentiment sentiment)
    {
        return _gauges[sentiment].level;
    }

    public Relationship(AllyCharacter source, AllyCharacter target)
    {
        this._source = source;
        this._target = target;
        _listEmotions = new List<EnumEmotions>();
        _gauges = new Dictionary<EnumSentiment, Gauge>();
        foreach (EnumSentiment sentiment in Enum.GetValues(typeof(EnumSentiment)))
        {
            _gauges.Add(sentiment, new Gauge());
        }
    }

    /// <summary>
    /// Increases the gauge representing the <c>sentiment</c> by the amount
    /// indicated by <c>gain</c>, and updates the level of the gauge
    /// if necessary.
    /// </summary>
    public void IncreaseSentiment(EnumSentiment sentiment, int gain, bool checkPrejudice = true)
    {
        if (checkPrejudice && gain < 0 && IsFeeling(EnumEmotions.Prejudice))
        {
            List<EnumClasses> against = new List<EnumClasses> { _target.CharacterClass };
            switch (_target.CharacterClass)
            {
                case EnumClasses.Berserker:
                    against.Add(EnumClasses.Engineer);
                    break;
                case EnumClasses.Engineer:
                    against.Add(EnumClasses.Berserker);
                    break;
                case EnumClasses.Sniper:
                    against.Add(EnumClasses.Alchemist);
                    break;
                case EnumClasses.Alchemist:
                    against.Add(EnumClasses.Sniper);
                    break;
                case EnumClasses.Bodyguard:
                    against.Add(EnumClasses.Smuggler);
                    break;
                case EnumClasses.Smuggler:
                    against.Add(EnumClasses.Bodyguard);
                    break;
                default:
                    break;
            }

            foreach (AllyUnit unit in CombatGameManager.Instance.AllAllyUnits)
            {
                AllyCharacter character = unit.AllyCharacter;
                if (character == _target || character == _source) continue;

                if (against.Contains(character.CharacterClass))
                {
                    _source.Relationships[character].IncreaseSentiment(sentiment, gain, false);
                }
            }
        }

        Gauge gauge = _gauges[sentiment];

        List<Trait> listeSelfTrait = _source.Traits;
        foreach (Trait trait in listeSelfTrait)
        {
            gain = trait.GetSelfToAllySentimentGain(sentiment, gain);
        }

        List<Trait> listeAllyTrait = _target.Traits;
        foreach (Trait trait in listeAllyTrait)
        {
            gain = trait.GetAllyToSelfSentimentGain(sentiment, gain);
        }

        int sentimentTotal = gauge.value + gain;

        if (sentimentTotal >= GetGaugeLimit(gauge.level))
        {
            if (gauge.level == 2)
            {
                gauge.value = 30;
            }
            else
            {
                gauge.level++;
                gauge.value = 0;
            }
        }
        else if (sentimentTotal <= -GetGaugeLimit(gauge.level))
        {
            if (gauge.level == -2)
            {
                gauge.value = -30;
            }
            else
            {
                gauge.level--;
                gauge.value = 0;
            }
        }
        else
        {
            gauge.value = sentimentTotal;
        }

        UpdateEmotions();
    }

    /// <summary>
    /// Return the limit of the value of a gauge, depending on the gauge level.
    /// If the value reaches the limit, the level is increased of decreased accordingly.
    /// </summary>
    private int GetGaugeLimit(int level)
    {
        int result = 0;

        switch (level)
        {
            case 0:
                result = 10;
                break;
            case 1:
            case -1:
                result = 20;
                break;
            case 2:
            case -2:
                result = 30;
                break;
            default :
                Debug.Log("Error : Gauge level cannot go below -2 or exceed 2");
                break;
        }

        return result;
    }

    public bool IsFeeling(EnumEmotions emotion)
    {
        return _listEmotions.Contains(emotion);
    }

    /// <summary>
    /// Updates the Emotions felt towards the target according to the
    /// current level of each sentiment gauge.
    /// </summary>
    public void UpdateEmotions()
    {
        List<EnumEmotions> old = new List<EnumEmotions>(_listEmotions);
        _listEmotions.Clear();

        // Combination of level 2 emotions
        if (_admirationGauge.level == 2)
        {
            if (_trustGauge.level == 2) _listEmotions.Add(EnumEmotions.Faith);
            else if (_trustGauge.level == -2) _listEmotions.Add(EnumEmotions.Submission);

            if (_sympathyGauge.level == 2) _listEmotions.Add(EnumEmotions.Devotion);
            else if (_sympathyGauge.level == -2) _listEmotions.Add(EnumEmotions.Recognition);
        }
        else if (_admirationGauge.level == -2)
        {
            if (_trustGauge.level == 2) _listEmotions.Add(EnumEmotions.ConflictedFeelings);
            else if (_trustGauge.level == -2) _listEmotions.Add(EnumEmotions.Prejudice);

            if (_sympathyGauge.level == 2) _listEmotions.Add(EnumEmotions.Pity);
            else if (_sympathyGauge.level == -2) _listEmotions.Add(EnumEmotions.Condescension);
        }

        if (_trustGauge.level == 2)
        {
            if (_sympathyGauge.level == 2) _listEmotions.Add(EnumEmotions.Friendship);
            else if (_sympathyGauge.level == -2) _listEmotions.Add(EnumEmotions.ReluctantTrust);
        }
        else if (_trustGauge.level == -2)
        {
            if (_sympathyGauge.level == 2) _listEmotions.Add(EnumEmotions.Apprehension);
            else if (_sympathyGauge.level == -2) _listEmotions.Add(EnumEmotions.Hate);
        }

        // If there are no combinations, then there is maximum one level 2 emotion
        if (_listEmotions.Count == 0)
        {
            if (_admirationGauge.level == 2) _listEmotions.Add(EnumEmotions.Esteem);
            else if (_admirationGauge.level == -2) _listEmotions.Add(EnumEmotions.Scorn);
            else if (_trustGauge.level == 2) _listEmotions.Add(EnumEmotions.Respect);
            else if (_trustGauge.level == -2) _listEmotions.Add(EnumEmotions.Terror);
            else if (_sympathyGauge.level == 2) _listEmotions.Add(EnumEmotions.Empathy);
            else if (_sympathyGauge.level == -2) _listEmotions.Add(EnumEmotions.Hostility);
        }

        // Finally, we add the level 1 emotions
        if (_admirationGauge.level == 1) _listEmotions.Add(EnumEmotions.Admiration);
        if (_admirationGauge.level == -1) _listEmotions.Add(EnumEmotions.Disdain);
        if (_trustGauge.level == 1) _listEmotions.Add(EnumEmotions.Trust);
        if (_trustGauge.level == -1) _listEmotions.Add(EnumEmotions.Fear);
        if (_sympathyGauge.level == 1) _listEmotions.Add(EnumEmotions.Sympathy);
        if (_sympathyGauge.level == -1) _listEmotions.Add(EnumEmotions.Antipathy);

        var lost = old.Except(_listEmotions).ToList();
        var gained = _listEmotions.Except(old).ToList();

        if ((lost.Count == 0 && gained.Count == 0) || !GlobalGameManager.Instance.InCombat) return;

        HistoryConsole.Instance
            .BeginEntry()
            .OpenLinkTag(_source.Name, CombatGameManager.Instance.GetUnitFromCharacter(_source), EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_source.FirstName).CloseTag()
            .AddText(" now ");

        if (lost.Count > 0)
        {
            HistoryConsole.Instance.AddText("no longer feels ");
            for (int i = 0; i < lost.Count - 1; i++)
            {
                EnumEmotions emotion = lost[i];
                HistoryConsole.Instance
                    .OpenColorTag(EmotionColor(emotion))
                    .AddText(emotion.ToString())
                    .CloseTag()
                    .AddText(", ");
            }
            if (lost.Count > 1) HistoryConsole.Instance.AddText("nor ");

            HistoryConsole.Instance
                .OpenColorTag(EmotionColor(lost[lost.Count - 1]))
                .AddText(lost[lost.Count - 1].ToString())
                .CloseTag();
        }

        if (gained.Count > 0)
        {
            if (lost.Count > 0) HistoryConsole.Instance.AddText(", but instead ");

            HistoryConsole.Instance.AddText("feels ");
            for (int i = 0; i < gained.Count - 1; i++)
            {
                EnumEmotions emotion = gained[i];
                HistoryConsole.Instance
                    .OpenColorTag(EmotionColor(emotion))
                    .AddText(emotion.ToString())
                    .CloseTag()
                    .AddText(", ");
            }
            if (gained.Count > 1) HistoryConsole.Instance.AddText("and ");

            HistoryConsole.Instance
                .OpenColorTag(EmotionColor(gained[gained.Count - 1]))
                .AddText(gained[gained.Count - 1].ToString())
                .CloseTag();
        }

        HistoryConsole.Instance
            .AddText(" towards ")
            .OpenLinkTag(_target.Name, CombatGameManager.Instance.GetUnitFromCharacter(_target), EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER).AddText(_target.FirstName).CloseTag()
            .Submit(2);
    }

    /// <summary>
    /// Get the status of the relationship
    /// </summary>
    /// <returns>-1 if negative relationship, 0 if neutral, 1 if positive</returns>
    public int Status()
    {
        int total = 0;
        foreach (int i in Enum.GetValues(typeof(EnumSentiment)))
        {
            //total += GetTotalGaugeValue((EnumSentiment)i);
            total += GetGaugeLevel((EnumSentiment)i);
        }
        return Mathf.Clamp(total, -1, 1);
    }

    private int EmotionStatus(EnumEmotions emotion)
    {
        switch (emotion)
        {
            case EnumEmotions.Disdain:
            case EnumEmotions.Scorn:
            case EnumEmotions.Fear:
            case EnumEmotions.Terror:
            case EnumEmotions.Antipathy:
            case EnumEmotions.Hostility:
            case EnumEmotions.Prejudice:
            case EnumEmotions.Condescension:
            case EnumEmotions.Hate:
                return -1;

            case EnumEmotions.Admiration:
            case EnumEmotions.Esteem:
            case EnumEmotions.Trust:
            case EnumEmotions.Respect:
            case EnumEmotions.Sympathy:
            case EnumEmotions.Empathy:
            case EnumEmotions.Faith:
            case EnumEmotions.Devotion:
            case EnumEmotions.Friendship:
                return 1;

            default:
                return 0;
        }
    }

    private Color EmotionColor(EnumEmotions emotion)
    {
        int status = EmotionStatus(emotion);
        switch (status)
        {
            case -1:
                return EntryColors.TEXT_NEGATIVE_EMOTION;
            case 1:
                return EntryColors.TEXT_POSITIVE_EMOTION;
            default:
                return EntryColors.TEXT_NEUTRAL_EMOTION;
        }
    }
}