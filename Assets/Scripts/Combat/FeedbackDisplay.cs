using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FeedbackDisplay : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private TMP_Text _text;

    private void Awake()
    {
        _canvasGroup = transform.Find("FeedbackCanvas").GetComponent<CanvasGroup>();
        _text = _canvasGroup.transform.Find("Text").GetComponent<TMP_Text>();
    }

    private IEnumerator DisplayFeedbackCoroutine()
    {
        for (float ft = 2f; ft >= 0; ft -= 0.01f)
        {
            _canvasGroup.alpha = ft / 2;
            _canvasGroup.transform.position += new Vector3(0, 0.01f, 0);
            yield return new WaitForSeconds(.01f);
        }
        _canvasGroup.transform.position = transform.position + new Vector3(0, 3, 0);
    }

    public void DisplayFeedback(string text)
    {
        _text.text = text;
        StartCoroutine(DisplayFeedbackCoroutine());
    }
}
