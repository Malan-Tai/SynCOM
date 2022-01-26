using UnityEngine;

public struct InterruptionParameters
{
    // parameters available to the scriptable objects
    public InterruptionType interruptionType;

    public float time;
    public PathfindingMoveType pathfinding;

    // parameters computed at runtime
    public GridBasedUnit target;
    public Vector2Int position;

    public string text;
    public Sprite sprite;
}

public enum PathfindingMoveType
{
    Astar,
    Linear
}