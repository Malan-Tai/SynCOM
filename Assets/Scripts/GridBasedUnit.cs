using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBasedUnit : MonoBehaviour
{
    private Vector2Int _gridPosition;
    private Vector3 _targetWorldPosition;
    [SerializeField]
    private float _moveSpeed;

    private void Start()
    {
        GridMap gridMap = GameManager.Instance.gridMap;
        print(transform.position);

        _gridPosition = gridMap.WorldToGrid(this.transform.position);
        this.transform.position = gridMap.GridToWorld(_gridPosition, this.transform.position.y);
        _targetWorldPosition = this.transform.position;

        print(_gridPosition);
        print(transform.position);
    }

    private float _timer = 0f;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > 1f)
        {
            MoveToNeighbor(new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2)));
            _timer = 0f;
        }

        Vector3 difference = _targetWorldPosition - this.transform.position;
        if (difference.sqrMagnitude > GameManager.Instance.gridMap.CellSize / 100f)
        {
            Vector3 movement = difference.normalized * _moveSpeed * Time.deltaTime;
            this.transform.position += movement;
        }
    }

    public void MoveToNeighbor(Vector2Int deltaGrid)
    {
        _gridPosition += deltaGrid;
        _targetWorldPosition = GameManager.Instance.gridMap.GridToWorld(_gridPosition, this.transform.position.y);
    }
}
