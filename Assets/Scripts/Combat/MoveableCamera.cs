using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableCamera : MonoBehaviour
{
    private Vector3 _startingOffset;
    private Vector3 _startingCameraOffset;
    private Camera _camera;
    private float _startingCamSize;

    [SerializeField]
    private float _minCameraSize;
    [SerializeField]
    private float _maxCameraSize;
    [SerializeField]
    private float _zoomSpeed;
    [SerializeField]
    private float _moveSpeed;
    [SerializeField]
    private float _rotationSpeed;

    private Vector3 _targetPosition;
    private bool _followTarget = false;

    private float _rotationSign;
    private float _targetRotationY;
    private bool _followRotation = false;

    private GridBasedUnit _parentViewedUnit;
    private GridBasedUnit _currentlyViewedUnit;

    private void Start()
    {
        _startingOffset = this.transform.localPosition;
        _camera = GetComponentInChildren<Camera>();
        _startingCameraOffset = _camera.transform.localPosition;
        _startingCamSize = _camera.orthographicSize;
        _targetRotationY = this.transform.localEulerAngles.y;

        _currentlyViewedUnit = GetComponentInParent<GridBasedUnit>();
        _parentViewedUnit = _currentlyViewedUnit;
    }

    public void EdgeMove(float x, float y)
    {
        float currentRotationY = this.transform.localEulerAngles.y;
        Vector3 mouseDelta = Vector3.zero;

        if (x < 0)
        {
            mouseDelta.x += -1f * Mathf.Cos(currentRotationY * Mathf.Deg2Rad);
            mouseDelta.y += 1f * Mathf.Sin(currentRotationY * Mathf.Deg2Rad);
        }
        else if (x > 0)
        {
            mouseDelta.x += 1f * Mathf.Cos(currentRotationY * Mathf.Deg2Rad);
            mouseDelta.y += -1f * Mathf.Sin(currentRotationY * Mathf.Deg2Rad);
        }
        if (y < 0)
        {
            mouseDelta.x += -1f * Mathf.Sin(currentRotationY * Mathf.Deg2Rad);
            mouseDelta.y += -1f * Mathf.Cos(currentRotationY * Mathf.Deg2Rad);
        }
        else if (y > 0)
        {
            mouseDelta.x += 1f * Mathf.Sin(currentRotationY * Mathf.Deg2Rad);
            mouseDelta.y += 1f * Mathf.Cos(currentRotationY * Mathf.Deg2Rad);
        }

        if (mouseDelta != Vector3.zero)
        {
            _followTarget = false;
            transform.position += new Vector3(mouseDelta.x, 0, mouseDelta.y).normalized * (_camera.orthographicSize / _minCameraSize) * _moveSpeed / 30;
        }
    }

    private void Update()
    {
        float currentRotationY = this.transform.localEulerAngles.y;

        if (_followRotation)
        {
            float sign = Mathf.Sign(_targetRotationY - currentRotationY) * _rotationSign;

            if (_targetRotationY - currentRotationY != 0f)
            {
                currentRotationY += _rotationSign * _rotationSpeed * Time.deltaTime;
            }

            if (_targetRotationY - currentRotationY == 0f || Mathf.Sign(_targetRotationY - currentRotationY) * _rotationSign != sign)
            {
                currentRotationY = _targetRotationY;
                _followRotation = false;
            }

            Vector3 oldEuler = this.transform.localEulerAngles;
            Vector3 euler = new Vector3(oldEuler.x, currentRotationY, oldEuler.z);
            this.transform.localRotation = Quaternion.Euler(euler);
        }

        if (_followTarget)
        {
            Vector3 delta = (_targetPosition - this.transform.localPosition) * _moveSpeed * Time.deltaTime;
            this.transform.localPosition += delta;

            if ((_targetPosition - this.transform.localPosition).magnitude <= 0.1f)
            {
                this.transform.localPosition = _targetPosition;
                _followTarget = false;
            }
        }
    }

    public float GetRotationY()
    {
        return this.transform.eulerAngles.y;
    }

    public void ResetCamera()
    {
        this.transform.localPosition = _startingOffset;
        _camera.orthographicSize = _startingCamSize;
    }

    public void SwitchParenthood(GridBasedUnit newUnit)
    {
        if (_currentlyViewedUnit == newUnit)
        {
            this.transform.SetParent(newUnit.transform, true);
        }
        else
        {
            Vector3 delta = newUnit.transform.position - _currentlyViewedUnit.transform.position;
            this.transform.localPosition -= delta;

            this.transform.SetParent(newUnit.transform, false);
        }
        _targetPosition = _startingOffset;
        _followTarget = true;

        _parentViewedUnit.InfoSetSmall(true);
        _currentlyViewedUnit.InfoSetSmall(true);
        if (_parentViewedUnit is EnemyUnit) (_parentViewedUnit as EnemyUnit).DisplayUnitSelectionTile(false);
        if (_currentlyViewedUnit is EnemyUnit) (_currentlyViewedUnit as EnemyUnit).DisplayUnitSelectionTile(false);

        _parentViewedUnit = newUnit;
        _currentlyViewedUnit = newUnit;

        _currentlyViewedUnit.InfoSetBig(true);
        if (_currentlyViewedUnit is EnemyUnit) (_currentlyViewedUnit as EnemyUnit).DisplayUnitSelectionTile(true);
    }

    public void SwitchViewWithoutParenthood(GridBasedUnit unit)
    {
        if (unit == _parentViewedUnit) return;

        _targetPosition = unit.transform.position + _startingOffset - _currentlyViewedUnit.transform.position;
        _followTarget = true;

        _currentlyViewedUnit = unit;

        _parentViewedUnit.InfoSetSmall(true);
        _currentlyViewedUnit.InfoSetBig(true);
    }

    public void SwitchViewBackToParent()
    {
        if (_currentlyViewedUnit == _parentViewedUnit) return;

        _targetPosition = _startingOffset;
        _followTarget = true;

        _parentViewedUnit.InfoSetBig(true);
        _currentlyViewedUnit.InfoSetSmall(true);

        _currentlyViewedUnit = _parentViewedUnit;
    }

    public void RotateCamera(float sign)
    {
        _targetRotationY += sign * 90f;
        _rotationSign = sign;

        if (_targetRotationY == -45f) _targetRotationY = 315f;
        else if (_targetRotationY == 405) _targetRotationY = 45f;

        _followRotation = true;
    }

    public void ZoomCamera(float delta)
    {
        float newSize = _camera.orthographicSize + delta * _zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(newSize, _minCameraSize, _maxCameraSize);
    }
}
