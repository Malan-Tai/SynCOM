using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class Pathfinder
{
    private List<Tile> _reachable;
    //public Dictionary<Tile, bool[]> Edges { get; set; }

    // paths contains, for every tile in reachable, a list of cell positions leading to said tile.
    // The first tile in the list is the first tile on the path, the last tile in the list is the target reachable tile.
    private Dictionary<Tile, List<Vector2Int>> _paths;
    private Dictionary<Tile, float> _costs;

    public List<Vector2Int> GetPathToTile(Vector2Int cell, out float cost)
    {
        Tile tile = CombatGameManager.Instance.GridMap[cell];
        if (!_reachable.Contains(tile))
        {
            cost = 0f;
            return new List<Vector2Int>();
        }

        cost = _costs[tile];
        return _paths[tile];
    }

    public bool CanReachCell(Vector2Int cell)
    {
        return _reachable.Contains(CombatGameManager.Instance.GridMap[cell]);
    }

    private float Heuristic(Vector2Int u, Vector2Int v)
    {
        return Mathf.Abs(u.x - v.x) + Mathf.Abs(u.y - v.y);
    }

    public Vector2Int[] AstarPath(Vector2Int start, Vector2Int goal)
    {
        GridMap map = CombatGameManager.Instance.GridMap;
        Tile startTile = map[start];
        Tile goalTile = map[goal];

        if (startTile == null || goalTile == null) return new Vector2Int[0];

        FastPriorityQueue<Tile> frontier = new FastPriorityQueue<Tile>(50); //50 probably won't be enough
        frontier.Enqueue(startTile, 0.0f);

        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, float> costSoFar = new Dictionary<Tile, float>();

        cameFrom[startTile] = null;
        costSoFar[startTile] = 0.0f;

        Tile current = startTile;
        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();
            if (current == goalTile)
            {
                break;
            }

            bool breakWhile = false;
            foreach (Tile next in map.MovementNeighbors(current.Coords, goal))
            {
                if (next == goalTile && !CombatGameManager.Instance.GridMap.CanMoveFromCellToCell(current.Coords, goal))
                {
                    // if next is the goal but cannot be walked, stop the pathfinding here : we are the closest to an unatteignable goal
                    breakWhile = true;
                    break;
                }

                float newCost = costSoFar[current] + next.MoveCost(current.Coords);
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float heurCost = Heuristic(next.Coords, goalTile.Coords);
                    float prio = newCost + heurCost;

                    frontier.Enqueue(next, prio);
                    cameFrom[next] = current;
                }
            }

            if (breakWhile) break;
        }

        Stack<Vector2Int> path = new Stack<Vector2Int>();
        if (current.Cover != EnumCover.None)
        {
            current = cameFrom[current];
        }
        path.Push(current.Coords);

        while (current != startTile)
        {
            Tile former = current;
            current = cameFrom[former];
            path.Push(current.Coords);
        }

        Vector2Int first = path.Peek();
        if (first == start) path.Pop(); // removes start

        return path.ToArray();
    }

    public void Dijkstra(float moves, Vector2Int start)
    {
        GridMap map = CombatGameManager.Instance.GridMap;
        Tile startTile = map[start];

        _reachable = new List<Tile> { startTile };
        //Edges = new Dictionary<Tile, bool[]>();
        _paths = new Dictionary<Tile, List<Vector2Int>>();
        _costs = new Dictionary<Tile, float>();

        if (startTile == null || moves < 1f) return;

        //50 is not enough, (int)moves * 10 might be ? -> It wasn't enough so I increased it to (int)moves * 11
        FastPriorityQueue<Tile> frontier = new FastPriorityQueue<Tile>(11 * (int)moves);
        frontier.Enqueue(startTile, 0.0f);

        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, float> costSoFar = new Dictionary<Tile, float>();

        cameFrom[startTile] = null;
        costSoFar[startTile] = 0.0f;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            foreach (Tile next in map.MovementNeighbors(current.Coords))
            {
                float newCost = costSoFar[current] + next.MoveCost(current.Coords);
                if (newCost > moves) continue;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float prio = newCost;

                    frontier.Enqueue(next, prio);
                    cameFrom[next] = current;

                    if (!_reachable.Contains(next) && next.Cover == EnumCover.None) _reachable.Add(next);
                }
            }
        }

        foreach (Tile tile in _reachable)
        {
            List<Vector2Int> path = new List<Vector2Int> { tile.Coords };
            Tile cur = tile;
            float cost = 0;

            while (cur != startTile)
            {
                Tile parent = cameFrom[cur];
                path.Insert(0, parent.Coords);
                cost += cur.MoveCost(parent.Coords);
                cur = parent;
            }

            _paths.Add(tile, path);
            _costs.Add(tile, cost);

            /// use if edges are important to display
            //bool[] edge = map.Neighbors(tile.Coords);
            //bool notInEdge = true;
            //for (int i = 0; i < 4; i++)
            //{
            //    notInEdge = notInEdge && edge[i];
            //}

            //if (!notInEdge) Edges.Add(tile, edge);
        }
    }

    public List<Tile> GetReachableTiles()
    {
        return _reachable;
    }
}
