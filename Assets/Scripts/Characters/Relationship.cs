using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Relationship
{
    /// <summary>
    /// The <c>Character</c> towards which the sentiments and emotions are felt.
    /// </summary>
    private Character _target;
    public Character Target
    {
        get { return _target; }
        private set { _target = value; }
    }

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

    public Relationship()
    {
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
    public void IncreaseSentiment(EnumSentiment sentiment, int gain)
    {
        Gauge gauge = _gauges[sentiment];
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
    }

    /// <summary>
    /// Return the limit of the value of a gauge, depdning on the gauge level.
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
    private void UpdateEmotions()
    {
        _listEmotions.Clear();

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

        if (_listEmotions.Count == 0)
        {
            if (_admirationGauge.level == 2) _listEmotions.Add(EnumEmotions.Esteem);
            else if (_admirationGauge.level == -2) _listEmotions.Add(EnumEmotions.Scorn);
            else if (_trustGauge.level == 2) _listEmotions.Add(EnumEmotions.Respect);
            else if (_trustGauge.level == -2) _listEmotions.Add(EnumEmotions.Terror);
            else if (_sympathyGauge.level == 2) _listEmotions.Add(EnumEmotions.Empathy);
            else if (_sympathyGauge.level == -2) _listEmotions.Add(EnumEmotions.Hostility);
        }
    }
}
