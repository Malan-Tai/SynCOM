using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasMover : MonoBehaviour
{
    [SerializeField]
    private LinkedCanvas _currentCanvas;

    private RectTransform _rect;

    private Vector3 _targetPosition;

    [SerializeField]
    private float _speed;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _targetPosition = _currentCanvas.RectTransform.position;
    }

    public void ChangeCanvas(int x, int y)
    {
        LinkedCanvas newCanvas = _currentCanvas.GetNextCanvas(x, y);

        if (newCanvas != null)
        {
            //_targetPosition = newCanvas.RectTransform.position;
            _currentCanvas = newCanvas;
            //print(_targetPosition);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeCanvas(0, 1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeCanvas(0, -1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeCanvas(-1, 0);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeCanvas(1, 0);

        //_rect.position += (_targetPosition - _rect.position) * _speed;
    }
}
