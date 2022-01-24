using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeedbackDisplay : MonoBehaviour
{
    private CanvasGroup _canvasGroupOne;
    private CanvasGroup _canvasGroupTwo;
    private TMP_Text _textOne;
    private TMP_Text _textTwo;
    private CanvasGroup _canvasGroupImage;
    private Image _image;
    private CanvasGroup _imageCanvasGroupOne;
    private CanvasGroup _imageCanvasGroupTwo;
    private Image _imageOne;
    private Image _imageTwo;

    private void Start()
    {
        _canvasGroupOne = transform.Find("FeedbackCanvas1").GetComponent<CanvasGroup>();
        _textOne = _canvasGroupOne.transform.Find("Text").GetComponent<TMP_Text>();
        _canvasGroupTwo = transform.Find("FeedbackCanvas2").GetComponent<CanvasGroup>();
        _textTwo = _canvasGroupTwo.transform.Find("Text").GetComponent<TMP_Text>();
        _canvasGroupImage = transform.Find("FeedbackImage").GetComponent<CanvasGroup>();
        _image = _canvasGroupImage.transform.Find("Image").GetComponent<Image>();
        _imageCanvasGroupOne = transform.Find("FeedbackRaisingImage1").GetComponent<CanvasGroup>();
        _imageOne = _imageCanvasGroupOne.transform.Find("Image").GetComponent<Image>();
        _imageCanvasGroupTwo = transform.Find("FeedbackRaisingImage2").GetComponent<CanvasGroup>();
        _imageTwo = _imageCanvasGroupTwo.transform.Find("Image").GetComponent<Image>();
    }

    private IEnumerator DisplayFeedbackCoroutine(int index)
    {
        CanvasGroup canvas = index == 0 ? _canvasGroupOne : _canvasGroupTwo;
        TMP_Text text = index == 0 ? _textOne : _textTwo;

        for (float ft = 2f; ft >= 0; ft -= 0.01f)
        {
            canvas.alpha = ft / 2;
            canvas.transform.position += new Vector3(0, 0.01f, 0);
            yield return new WaitForSeconds(.01f);
        }

        canvas.transform.position = canvas.transform.position - new Vector3(0, 2, 0);
        text.text = "free";
    }

    public void DisplayFeedback(string text)
    {
        if (_textOne.text == "free")
        {
            _textOne.text = text;
            StartCoroutine(DisplayFeedbackCoroutine(0));
        }
        else
        {
            _textTwo.text = text;
            StartCoroutine(DisplayFeedbackCoroutine(1));
        }
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
        StartCoroutine(DisplayImageFeedbackCoroutine());
    }

    private IEnumerator DisplayRaisingImageFeedbackCoroutine(int index)
    {
        CanvasGroup canvas = index == 0 ? _imageCanvasGroupOne : _imageCanvasGroupTwo;
        Image image = index == 0 ? _imageOne : _imageTwo;

        for (float ft = 2f; ft >= 0; ft -= 0.01f)
        {
            canvas.alpha = ft / 2;
            canvas.transform.position += new Vector3(0, 0.01f, 0);
            yield return new WaitForSeconds(.01f);
        }

        canvas.transform.position = canvas.transform.position - new Vector3(0, 2, 0);
        image.sprite = null;
    }

    public void DisplayRaisingImageFeedback(Sprite sprite)
    {
        if (_imageOne.sprite == null)
        {
            _imageOne.sprite = sprite;
            StartCoroutine(DisplayRaisingImageFeedbackCoroutine(0));
        }
        else
        {
            _imageTwo.sprite = sprite;
            StartCoroutine(DisplayRaisingImageFeedbackCoroutine(1));
        }
    }
}
