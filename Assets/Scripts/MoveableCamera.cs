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
        _startingOffset = this.transform.position;
        _camera = GetComponentInChildren<Camera>();
        _startingCamSize = _camera.orthographicSize;
    }

    public void ResetCamera()
    {
        this.transform.position = _startingOffset;
        _camera.orthographicSize = _startingCamSize;
    }

    public void SwitchParenthood(GridBasedUnit newUnit)
    {
        this.transform.position = _startingOffset;
        this.transform.SetParent(newUnit.transform);
    }
}