using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBasedUnit : MonoBehaviour
{
    private Vector2 _gridPosition;

    private void Start()
    {
        GridMap gridMap = GameManager.Instance.gridMap;
        print(transform.position);

        _gridPosition = gridMap.WorldToGrid(this.transform.position);
        this.transform.position = gridMap.GridToWorld(_gridPosition, this.transform.position.y);

        print(_gridPosition);
        print(transform.position);
    }

    private float _timer = 0f;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > 1f)
        {
            _gridPosition += new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
            _timer = 0f;

            this.transform.position = GameManager.Instance.gridMap.GridToWorld(_gridPosition, this.transform.position.y);
        }
    }
}
