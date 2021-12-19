using TMPro;
using UnityEngine;

public class MoneyText : MonoBehaviour
{
    private TMP_Text tmp_Text;
    private string _startText;

    private void Start()
    {
        tmp_Text = GetComponent<TMP_Text>();

        _startText = tmp_Text.text;
        UpdateMoney();
    }

    public void UpdateMoney()
    {
        tmp_Text.text = $"{_startText}{GlobalGameManager.Instance.Money}";
    }
}
