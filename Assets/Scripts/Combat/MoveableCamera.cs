using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableCamera : MonoBehaviour
{
    private Vector3 _startingOffset;
    private Camera _camera;
    private float _startingCamSize;

    private void Start()
    {
        _startingOffset = this.transform.localPosition;
        _camera = GetComponentInChildren<Camera>();
        _startingCamSize = _camera.orthographicSize;
    }

    public void ResetCamera()
    {
        this.transform.localPosition = _startingOffset;
        _camera.orthographicSize = _startingCamSize;
    }

    public void SwitchParenthood(GridBasedUnit newUnit)
    {
        this.transform.localPosition = _startingOffset;
        this.transform.SetParent(newUnit.transform, false);
    }

    public void RotateCamera(float sign)
    {
        Vector3 euler = this.transform.localEulerAngles + new Vector3(0, sign * 90, 0);
        this.transform.localRotation = Quaternion.Euler(euler);
    }
}
