using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeedbackDisplay : MonoBehaviour
{
    private CanvasGroup _canvasGroupOne;
    private CanvasGroup _canvasGroupTwo;
    private CanvasGroup _canvasGroupImage;
    private TMP_Text _textOne;
    private TMP_Text _textTwo;
    private Image _image;
    private IEnumerator DisplayFeedbackCoroutine()
    {
        for (float ft = 2f; ft >= 0; ft -= 0.01f)
        {
            _canvasGroupOne.alpha = ft / 2;
            _canvasGroupOne.transform.position += new Vector3(0, 0.01f, 0);

            if (_textTwo.text != "free")
                _canvasGroupTwo.alpha = ft / 2;
            
            _canvasGroupTwo.transform.position += new Vector3(0, 0.01f, 0);
            yield return new WaitForSeconds(.01f);
        }
        Debug.Log("bah alors");
        _canvasGroupOne.transform.position = _canvasGroupOne.transform.position - new Vector3(0, 2, 0);
        _textOne.text = "free";
        _canvasGroupTwo.transform.position = _canvasGroupTwo.transform.position - new Vector3(0, 2, 0);
        _textTwo.text = "free";
    }
    public void DisplayFeedback(string text)
    {
        _canvasGroupOne = transform.Find("FeedbackCanvas1").GetComponent<CanvasGroup>();
        _textOne = _canvasGroupOne.transform.Find("Text").GetComponent<TMP_Text>();
        _canvasGroupTwo = transform.Find("FeedbackCanvas2").GetComponent<CanvasGroup>();
        _textTwo = _canvasGroupTwo.transform.Find("Text").GetComponent<TMP_Text>();

        if (_textOne.text != "free")
            _textTwo.text = text;

        else
            _textOne.text = text;

        StartCoroutine(DisplayFeedbackCoroutine());
    }


    private IEnumerator DisplayImageFeedbackCoroutine()
    {
        for (float ft = 2f; ft >= 0; ft -= 0.01f)
        {
            _canvasGroupImage.alpha = ft / 2;
            yield return new WaitForSeconds(.01f);
        }
    }
    public void DisplayImageFeedback()
    {
        _canvasGroupImage = transform.Find("FeedbackImage").GetComponent<CanvasGroup>();
        _image = _canvasGroupImage.transform.Find("Image").GetComponent<Image>();
        

        StartCoroutine(DisplayImageFeedbackCoroutine());
    }

}
