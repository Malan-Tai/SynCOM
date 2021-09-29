using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Sentiment
{
    Admiration,
    Trust,
    Sympathy
}

public class Relationship
{
    private Character _target;
    public Character Target
    {
        get { return _target; }
        private set { _target = value; }
    }

    private class Gauge
    {
        public int level;
        public int value;

        public Gauge()
        {
            this.level = 0;
            this.value = 0;
        }
    }

    private Dictionary<Sentiment, Gauge> _gauges;
    private Gauge _admirationGauge { get { return _gauges[Sentiment.Admiration]; } }
    private Gauge _trustGauge { get { return _gauges[Sentiment.Trust]; } }
    private Gauge _sympathyGauge { get { return _gauges[Sentiment.Sympathy]; } }

    private List<EnumEmotions> _listEmotions;

    public Relationship()
    {
        _listEmotions = new List<EnumEmotions>();
        _gauges = new Dictionary<Sentiment, Gauge>();
        foreach (Sentiment sentiment in Enum.GetValues(typeof(Sentiment)))
        {
            _gauges.Add(sentiment, new Gauge());
        }
    }

    public void IncreaseSentiment(Sentiment sentiment, int gain)
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
