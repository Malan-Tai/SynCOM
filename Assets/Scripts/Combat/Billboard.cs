using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private MoveableCamera _camera;

    void Start()
    {
        _camera = Camera.main.GetComponentInParent<MoveableCamera>();
    }

    void LateUpdate()
    {
        this.transform.rotation = Quaternion.Euler(0f, _camera.GetRotationY(), 0f);
    }
}
