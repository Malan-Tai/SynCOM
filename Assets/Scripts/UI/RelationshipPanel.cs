using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelationshipPanel : MonoBehaviour
{
    private Image       _leftPortrait;
    private Image       _rightPortrait;
    private TMP_Text    _leftName;
    private TMP_Text    _rightName;
    private TMP_Text    _leftRightEmotion;
    private TMP_Text    _rightLeftEmotion;

    void Awake()
    {
        _leftPortrait       = transform.Find("LeftPortrait").GetComponent<Image>();
        _rightPortrait      = transform.Find("RightPortrait").GetComponent<Image>();
        _leftName           = transform.Find("LeftName").GetComponent<TMP_Text>();
        _rightName          = transform.Find("RightName").GetComponent<TMP_Text>();
        _leftRightEmotion   = transform.Find("LeftRightEmotion").GetComponent<TMP_Text>();
        _rightLeftEmotion   = transform.Find("RightLeftEmotion").GetComponent<TMP_Text>();
    }

    public void SetPanel(AllyCharacter left, AllyCharacter right)
    {
        _leftName.text = left.Name.Split(' ')[0];
        _leftPortrait.sprite = left.GetPortrait();
        _rightName.text = right.Name.Split(' ')[0];
        _rightPortrait.sprite = right.GetPortrait();

        List<EnumEmotions> leftRightEmotions = left.Relationships[right].ListEmotions;

        if (leftRightEmotions.Count > 0)
        {
            string leftRightText = "";
            foreach (EnumEmotions emotion in leftRightEmotions)
            {
                leftRightText += emotion.ToString() + ", ";
            }
            _leftRightEmotion.text = leftRightText.TrimEnd(' ', ',');
        }
        else _leftRightEmotion.text = "Neutral";


        List<EnumEmotions> rightLeftEmotions = right.Relationships[left].ListEmotions;

        if (rightLeftEmotions.Count > 0)
        {
            string rightLeftText = "";
            foreach (EnumEmotions emotion in rightLeftEmotions)
            {
                rightLeftText += emotion.ToString() + ", ";
            }
            _rightLeftEmotion.text = rightLeftText.TrimEnd(' ', ',');
        }
        else _rightLeftEmotion.text = "Neutral";
    }
}
