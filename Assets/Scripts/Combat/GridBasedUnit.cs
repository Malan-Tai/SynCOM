using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridBasedUnit : MonoBehaviour
{
    [SerializeField] private MeshRenderer _outlineRenderer;
    [SerializeField] private Material _outlineMaterial;

    private Vector2Int _gridPosition;
    private Vector3 _targetWorldPosition;

    public Vector2Int GridPosition { get { return _gridPosition; } }

    public InterruptionQueue InterruptionQueue { get; protected set; }

    protected Character _character;
    public Character Character
    { 
        get
        {
            return _character;
        }
        set
        {
            _character = value;
            _character.OnDeath += Die;
        }
    }

    [SerializeField]
    private float _moveSpeed;

    protected float _movesLeft;

    private Pathfinder _pathfinder = new Pathfinder();
    private bool _updatePathfinder = true;
    private List<Vector2Int> _pathToFollow;
    private bool _followingPath;

    protected Dictionary<GridBasedUnit, LineOfSight> _linesOfSight;
    private float _sightDistance;

    private bool _markedForDeath = false;

    public Dictionary<GridBasedUnit, LineOfSight> LinesOfSight { get { return _linesOfSight; } }

    public delegate void StartedMoving(GridBasedUnit movedUnit, Vector2Int finalPos);
    public static event StartedMoving OnMoveStart;

    public delegate void FinishedMoving(GridBasedUnit movedUnit);
    public static event FinishedMoving OnMoveFinish;

    public delegate void DieEvent(GridBasedUnit deadUnit);
    public static event DieEvent OnDeath;

    private FeedbackDisplay _feedback;

    protected SpriteRenderer _unitRenderer;
    private int _highlightPropertyHash;
    private int _highlightColorPropertyHash;
    private int _outlineColorPropertyHash;
    private int _outlineSizePropertyHash;

    protected InfoCanvas _info;

    protected void Start()
    {
        GridMap gridMap = CombatGameManager.Instance.GridMap;

        _gridPosition = gridMap.WorldToGrid(this.transform.position, true);
        this.transform.position = gridMap.GridToWorld(_gridPosition, this.transform.position.y);
        _targetWorldPosition = this.transform.position;

        _sightDistance = 20;

        _linesOfSight = new Dictionary<GridBasedUnit, LineOfSight>();

        _feedback = GetComponent<FeedbackDisplay>();

        _unitRenderer = transform.Find("Renderer").GetComponent<SpriteRenderer>();
        _highlightPropertyHash = Shader.PropertyToID("_Highlight");
        _highlightColorPropertyHash = Shader.PropertyToID("_HighlightColor");
        _outlineColorPropertyHash = Shader.PropertyToID("_OutlineColor");
        _outlineSizePropertyHash = Shader.PropertyToID("_OutlineSize");

        InterruptionQueue = GetComponent<InterruptionQueue>();

        _info = transform.Find("Renderer").GetComponentInChildren<InfoCanvas>();
    }

    protected virtual void Update()
    {
        if (_updatePathfinder)
        {
            _pathfinder.Dijkstra(_movesLeft, _gridPosition);
            _pathToFollow = new List<Vector2Int>();
            _updatePathfinder = false;
            _followingPath = false;

            if (CombatGameManager.Instance.CurrentUnit == this) CombatGameManager.Instance.UpdateReachableTiles();
        }

        Vector3 difference = _targetWorldPosition - this.transform.position;
        if (difference.sqrMagnitude > CombatGameManager.Instance.GridMap.CellSize / 100f)
        {
            Vector3 movement = difference.normalized * _moveSpeed * Time.deltaTime;
            this.transform.position += movement;
        }
        else if (_pathToFollow.Count > 0)
        {
            MoveToCell(_pathToFollow[0]);
            _pathToFollow.RemoveAt(0);
        }
        else if (_followingPath)
        {
            _updatePathfinder = true;
            UpdateLineOfSights(!IsEnemy());
            if (OnMoveFinish != null) OnMoveFinish(this);

            // Properly center the unit on the tile
            transform.position = CombatGameManager.Instance.GridMap.GridToWorld(GridPosition, transform.position.y);
        }

        if (_markedForDeath && InterruptionQueue.IsEmpty() && transform.Find("CameraTarget") == null)
        {
            _markedForDeath = false;
            _unitRenderer.enabled = false;
            _info.gameObject.SetActive(false);
            transform.Find("DeadRenderer").GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void GenerateOutlineTexture()
    {
        int outlineSize = _outlineMaterial.GetInt(_outlineSizePropertyHash);
        RenderTexture outlineTexture = new RenderTexture
        (
            4 * outlineSize + _unitRenderer.sprite.texture.width,
            4 * outlineSize + _unitRenderer.sprite.texture.height,
            0,
            RenderTextureFormat.ARGB32
        );
        outlineTexture.Create();

        Graphics.Blit(_unitRenderer.sprite.texture, outlineTexture, _outlineMaterial);

        _outlineRenderer.material.mainTexture = outlineTexture;
        _outlineRenderer.transform.localScale = new Vector3
        (
            outlineTexture.width / _unitRenderer.sprite.pixelsPerUnit,
            outlineTexture.height / _unitRenderer.sprite.pixelsPerUnit,
            1f
        );
        _outlineRenderer.enabled = false;
    }

    public void SetCharacter(Character character)
    {
        Character = character;
        _movesLeft = character.MovementPoints;
        InitSprite();
        _info.SetHP(Character.HealthPoints, Character.MaxHealth);
        _info.SetSmall(true);
        GenerateOutlineTexture();
    }

    public void MarkForDeath()
    {
        _markedForDeath = true;
    }

    protected virtual bool IsEnemy()
    {
        return false;
    }

    public void MoveToNeighbor(Vector2Int deltaGrid)
    {
        _gridPosition += deltaGrid;
        _targetWorldPosition = CombatGameManager.Instance.GridMap.GridToWorld(_gridPosition, this.transform.position.y);
    }

    public virtual void MoveToCell(Vector2Int cell, bool eventOnEnd = false)
    {
        _gridPosition = cell;
        _targetWorldPosition = CombatGameManager.Instance.GridMap.GridToWorld(_gridPosition, this.transform.position.y);
        _followingPath = _followingPath || eventOnEnd;
    }

    public void ChoosePathTo(Vector2Int cell)
    {
        if (_followingPath) return;

        float cost;
        _pathToFollow = _pathfinder.GetPathToTile(cell, out cost);
        _movesLeft -= cost;

        if (_pathToFollow.Count > 0)
        {
            _followingPath = true;
            CombatGameManager.Instance.GridMap.UpdateOccupiedTiles(_gridPosition, cell);

            if (OnMoveStart != null) OnMoveStart(this, cell);
        }
    }

    public bool ChooseAstarPathTo(Vector2Int cell)
    {
        if (_followingPath) return false;

        _pathToFollow = new List<Vector2Int>(_pathfinder.AstarPath(_gridPosition, cell));

        if (_pathToFollow.Count > 0)
        {
            _followingPath = true;
            CombatGameManager.Instance.GridMap.UpdateOccupiedTiles(_gridPosition, cell);

            if (OnMoveStart != null) OnMoveStart(this, cell);

            return true;
        }
        return false;
    }

    public void NeedsPathfinderUpdate()
    {
        _updatePathfinder = true;
    }

    public Dictionary<GridBasedUnit, LineOfSight> GetLineOfSights(bool targetEnemies)
    {
        GridMap map = CombatGameManager.Instance.GridMap;
        var result = new Dictionary<GridBasedUnit, LineOfSight>();

        List<GridBasedUnit> listToCycle = new List<GridBasedUnit>();
        if (targetEnemies)
        {
            foreach (GridBasedUnit enemy in CombatGameManager.Instance.EnemyUnits)
            {
                listToCycle.Add(enemy);
            }
        }
        else
        {
            foreach (GridBasedUnit ally in CombatGameManager.Instance.AllAllyUnits)
            {
                listToCycle.Add(ally);
            }
        }

        List<Vector2Int> shooterSidesteps = map.SidestepPositions(_gridPosition);
        shooterSidesteps.Insert(0, _gridPosition);

        foreach (GridBasedUnit unit in listToCycle)
        {
            List<CoverPlane> planes = map.GetCoverPlanes(unit._gridPosition);
            List<Vector2Int> targetSidesteps = map.SidestepPositions(unit._gridPosition);
            targetSidesteps.Insert(0, unit._gridPosition);

            LineOfSight bestLine = new LineOfSight();
            bestLine.seen = false;
            bestLine.cover = EnumCover.Full; // worst case for the shooter

            foreach (Vector2Int shooterSidestep in shooterSidesteps)
            {
                foreach (Vector2Int targetSidestep in targetSidesteps)
                {
                    LineOfSight newLine = ComputeLineOfSight(planes, shooterSidestep, targetSidestep, unit.transform.position.y);

                    if (newLine.seen && (!bestLine.seen || (int)newLine.cover < (int)bestLine.cover))
                    {
                        bestLine = newLine;
                    }
                }
            }

            if (bestLine.seen)
            {
                result.Add(unit, bestLine);
            }
        }

        return result;
    }

    public void UpdateLineOfSights(bool targetEnemies = true)
    {
        _linesOfSight = GetLineOfSights(targetEnemies);
    }

    private LineOfSight ComputeLineOfSight(List<CoverPlane> targetCoverPlanes, Vector2Int shooterPosition, Vector2Int targetPosition, float targetY)
    {
        GridMap map = CombatGameManager.Instance.GridMap;
        LayerMask layerMask = 1 << 6;

        LineOfSight lineOfSight = new LineOfSight();

        Vector3 lineEnd = map.GridToWorld(targetPosition, targetY);
        Vector3 lineStart = map.GridToWorld(shooterPosition, this.transform.position.y);
        Vector3 line = lineEnd - lineStart;
        bool seen = !Physics.Raycast(lineStart, line, Mathf.Min(line.magnitude, _sightDistance), layerMask);

        lineOfSight.seen = seen;

        if (!seen)
        {
            return lineOfSight;
        }

        EnumCover bestCover = EnumCover.None; // best cover for the target
        foreach (CoverPlane plane in targetCoverPlanes)
        {
            if (plane.IntersectsSegment(lineStart, lineEnd))
            {
                if (plane.cover == EnumCover.Full)
                {
                    bestCover = EnumCover.Full;
                    break;
                }
                else if (plane.cover == EnumCover.Half && bestCover == EnumCover.None)
                {
                    bestCover = EnumCover.Half;
                }
            }
        }

        lineOfSight.cover = bestCover;
        lineOfSight.sidestepCell = shooterPosition;

        return lineOfSight;
    }

    public virtual void NewTurn()
    {
        _movesLeft = _character.MovementPoints;
        NeedsPathfinderUpdate();
        UpdateLineOfSights(!IsEnemy());
    }

    public List<Tile> GetReachableTiles()
    {
        return _pathfinder.GetReachableTiles();
    }

    public void Missed()
    {
        _feedback.DisplayFeedback("Miss");
    }

    public bool TakeDamage(ref float damage, bool textFeedback = true, bool imgFeedback = true)
    {
        bool died = _character.TakeDamage(ref damage);
        _info.SetHP(Character.HealthPoints, Character.MaxHealth);
        if (textFeedback) _feedback.DisplayFeedback("-" + damage.ToString());
        if (imgFeedback) _feedback.DisplayImageFeedback();

        return died;
    }

    public void Heal(ref float healAmount, bool feedback = true)
    {
        _character.Heal(ref healAmount);
        _info.SetHP(Character.HealthPoints, Character.MaxHealth);
        if (feedback) _feedback.DisplayFeedback("+" + healAmount.ToString());
    }

    public void DisplayFeedback(string text)
    {
        _feedback.DisplayFeedback(text);
    }

    public void DisplayRaisingImageFeedback(Sprite sprite)
    {
        _feedback.DisplayRaisingImageFeedback(sprite);
    }

    private void Die()
    {
        if (OnDeath != null) OnDeath(this);
    }

    public virtual void InitSprite()
    {

    }

    public virtual Sprite GetPortrait()
    {
        return _character.GetPortrait();
    }

    public void HighlightUnit(Color highlightColor)
    {
        _unitRenderer.material.SetInt(_highlightPropertyHash, 1);
        _unitRenderer.material.SetColor(_highlightColorPropertyHash, highlightColor);
    }

    public void DontHighlightUnit()
    {
        _unitRenderer.material.SetInt(_highlightPropertyHash, 0);
    }

    public void DisplayOutline(bool display)
    {
        _outlineRenderer.enabled = display;
    }

    public void SetOutlineColor(Color color)
    {
        _outlineRenderer.material.SetColor(_outlineColorPropertyHash, color);
    }

    public void InfoSetSmall(bool force)
    {
        _info.SetSmall(force);
    }

    public void InfoSetBig(bool force)
    {
        _info.SetBig(force);
    }
}
