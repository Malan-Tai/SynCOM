using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridMap))]
public class GridMapEditor : Editor
{
    private GridMap _gridMap;

    private void OnSceneGUI()
    {
        _gridMap = (GridMap) target;

        Handles.BeginGUI();

        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = _gridMap.ShowGridGizmos ? Color.green : Color.red;
        if (GUI.Button(new Rect(5, 5, 50, 25), "Grid"))
        {
            _gridMap.ShowGridGizmos = !_gridMap.ShowGridGizmos;
        }

        GUI.backgroundColor = _gridMap.ShowWalkableGizmos ? Color.green : Color.red;
        if (GUI.Button(new Rect(5, 35, 50, 25), "Tiles"))
        {
            _gridMap.ShowWalkableGizmos = !_gridMap.ShowWalkableGizmos;
        }

        GUI.backgroundColor = _gridMap.ShowCoversGizmos ? Color.green : Color.red;
        if (GUI.Button(new Rect(5, 65, 50, 25), "Covers"))
        {
            _gridMap.ShowCoversGizmos = !_gridMap.ShowCoversGizmos;
        }

        Handles.EndGUI();
    }
}
