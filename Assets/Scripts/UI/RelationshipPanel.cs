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
        // TODO : portraits & names
        _leftPortrait.sprite = GlobalGameManager.Instance.GetClassPortrait(left.CharacterClass);
        _rightPortrait.sprite = GlobalGameManager.Instance.GetClassPortrait(right.CharacterClass);

        List<EnumEmotions> leftRightEmotions = left.Relationships[right].ListEmotions;

        if (leftRightEmotions.Count > 0)
        {
            string leftRightText = "";
            foreach (EnumEmotions emotion in leftRightEmotions)
            {
                leftRightText += emotion.ToString();
            }
            _leftRightEmotion.text = leftRightText;
        }
        else _leftRightEmotion.text = "Neutral";


        List<EnumEmotions> rightLeftEmotions = right.Relationships[left].ListEmotions;

        if (rightLeftEmotions.Count > 0)
        {
            string rightLeftText = "";
            foreach (EnumEmotions emotion in rightLeftEmotions)
            {
                rightLeftText += emotion.ToString();
            }
            _rightLeftEmotion.text = rightLeftText;
        }
        else _rightLeftEmotion.text = "Neutral";
    }
}
