using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasMover : MonoBehaviour
{
    [SerializeField]
    private LinkedCanvas _currentCanvas;
    private RectTransform _currentRect;

    private Vector3 _baseCameraPosition;
    private Vector3 _targetOffset;
    private Vector3 _currentOffset
    {
        get
        {
            return _camera.position - _baseCameraPosition;
        }
    }

    private Transform _camera;

    [SerializeField]
    private float _speed;

    private void Awake()
    {
        _camera = Camera.main.transform;
        _baseCameraPosition = _camera.position;
        _currentRect = _currentCanvas.RectTransform;
    }

    private void OnEnable()
    {
        RegionButton.OnMouseClickEvent += ClickRegion;
    }

    private void OnDisable()
    {
        RegionButton.OnMouseClickEvent -= ClickRegion;
    }

    private void ClickRegion()
    {
        ChangeCanvas(1, 0);
    }

    public void ChangeCanvas(int x, int y)
    {
        if ((_targetOffset - _currentOffset).sqrMagnitude > 5f) return;

        LinkedCanvas newCanvas = _currentCanvas.GetNextCanvas(x, y);

        if (newCanvas != null)
        {
            _targetOffset += newCanvas.RectTransform.position - _currentRect.position;

            _currentRect = newCanvas.RectTransform;
            _currentCanvas = newCanvas;
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeCanvas(0, 1);
        //else if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeCanvas(0, -1);
        //else if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeCanvas(-1, 0);
        //else if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeCanvas(1, 0);

        _camera.position += (_targetOffset - _currentOffset) * _speed * Time.deltaTime;
    }
}
