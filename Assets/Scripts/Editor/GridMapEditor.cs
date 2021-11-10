using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridMap))]
public class GridMapEditor : Editor
{
    private GridMap _gridMap;

    private void OnEnable()
    {
        _gridMap = (GridMap)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update grid"))
        {
            _gridMap.RecalculateGrid();
        }
    }

    private void OnSceneGUI()
    {
        Handles.BeginGUI();

        // Button to show/hide grid display
        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = _gridMap.ShowGridGizmos ? Color.green : Color.red;
        if (GUI.Button(new Rect(5, 5, 50, 25), "Grid"))
        {
            _gridMap.ShowGridGizmos = !_gridMap.ShowGridGizmos;
        }

        // Button to show/hide walkable tiles
        GUI.backgroundColor = _gridMap.ShowWalkableGizmos ? Color.green : Color.red;
        if (GUI.Button(new Rect(5, 35, 50, 25), "Tiles"))
        {
            _gridMap.ShowWalkableGizmos = !_gridMap.ShowWalkableGizmos;
        }

        // Button to show/hide covers
        GUI.backgroundColor = _gridMap.ShowCoversGizmos ? Color.green : Color.red;
        if (GUI.Button(new Rect(5, 65, 50, 25), "Covers"))
        {
            _gridMap.ShowCoversGizmos = !_gridMap.ShowCoversGizmos;
        }

        // Slider and text field for cell size
        GUI.backgroundColor = Color.blue;
        _gridMap.CellSize = GUI.HorizontalSlider(new Rect(60, 7.5f, 100, 20), _gridMap.CellSize, 0.2f, 10f);
        _gridMap.CellSize = float.Parse(GUI.TextField(new Rect(75, 30, 70, 20), _gridMap.CellSize.ToString()));

        // Button for updating the grid to display
        GUI.backgroundColor = Color.blue;
        if (GUI.Button(new Rect(70, 65, 80, 25), "Update grid"))
        {
            _gridMap.RecalculateGrid();
        }

        Handles.EndGUI();
    }
}
