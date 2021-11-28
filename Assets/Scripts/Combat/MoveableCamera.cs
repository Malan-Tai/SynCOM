using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableCamera : MonoBehaviour
{
    private Vector3 _startingOffset;
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
        _startingCamSize = _camera.orthographicSize;
        _targetRotationY = this.transform.localEulerAngles.y;
    }

    private void Update()
    {
        if (_followRotation)
        {
            float currentY = this.transform.localEulerAngles.y;
            float sign = Mathf.Sign(_targetRotationY - currentY) * _rotationSign;

            if (_targetRotationY - currentY != 0f)
            {
                currentY += _rotationSign * _rotationSpeed * Time.deltaTime;
            }

            if (_targetRotationY - currentY == 0f || Mathf.Sign(_targetRotationY - currentY) * _rotationSign != sign)
            {
                currentY = _targetRotationY;
                _followRotation = false;
            }

            Vector3 oldEuler = this.transform.localEulerAngles;
            Vector3 euler = new Vector3(oldEuler.x, currentY, oldEuler.z);
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
            Vector3 delta = newUnit.transform.position - this.transform.position;
            this.transform.localPosition -= delta;

            this.transform.SetParent(newUnit.transform, false);
        }
        _targetPosition = _startingOffset;
        _followTarget = true;

        _parentViewedUnit = newUnit;
        _currentlyViewedUnit = newUnit;
    }

    public void SwitchViewWithoutParenthood(GridBasedUnit unit)
    {
        _targetPosition = unit.transform.position + _startingOffset - transform.position;
        _followTarget = true;

        _currentlyViewedUnit = unit;
    }

    public void SwitchViewBackToParent()
    {
        _targetPosition = _startingOffset;
        _followTarget = true;

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
