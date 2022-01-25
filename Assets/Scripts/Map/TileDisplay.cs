using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class TileDisplay : MonoBehaviour
{
    [SerializeField] private float _displayHeight = 0.01f;
    [SerializeField, Range(1, 31)] private int _zonesChunkSizeInTiles = 30;
    [SerializeField] private Sprite _mouseHoverTileSprite;
    [SerializeField] private TileZonePreset[] _presets;
    [SerializeField] private MeshRenderer _tileZoneRendererPrefab;
    [SerializeField] private MeshRenderer _gridLinesRenderer;

    [SerializeField] private Sprite _fullCoverSprite;
    [SerializeField] private Sprite _halfCoverSprite;

    // Sprite renderers to render tiles
    private SpriteRenderer _mouseHovertileSpriteRenderer;
    private SpriteRenderer _upCoverRenderer;
    private SpriteRenderer _downCoverRenderer;
    private SpriteRenderer _leftCoverRenderer;
    private SpriteRenderer _rightCoverRenderer;

    private readonly Dictionary<string, MeshRenderer[]> _tileZonesRenderers = new Dictionary<string, MeshRenderer[]>();
    private ushort maxOrder = ushort.MinValue;
    private Vector2Int _previousMouseCoord = Vector2Int.zero;

    private void Start()
    {
        // Grid lines
        _gridLinesRenderer.material.SetFloat("_CellSize", CombatGameManager.Instance.GridMap.CellSize);

        // Create tile zones for the list of given presets
        foreach (TileZonePreset preset in _presets)
        {
            if (_tileZonesRenderers.ContainsKey(preset.Name))
            {
                Debug.LogWarning($"{preset.Name} found duplicated in tile zone presets, ensure that all presets have different keys/names");
            }
            else if (!CreateNewTileZone(preset))
            {
                Debug.LogWarning($"Can't create preset tilezone, check that name isn't empty and tileset isn't null");
            }
        }

        // Tile under mouse
        GameObject spriteRendererGO = new GameObject("MouseHoverTileSprite");
        spriteRendererGO.transform.parent = transform;
        _mouseHovertileSpriteRenderer = spriteRendererGO.AddComponent<SpriteRenderer>();
        _mouseHovertileSpriteRenderer.sprite = _mouseHoverTileSprite;
        _mouseHovertileSpriteRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        GameObject coverGO = new GameObject("upCover");
        coverGO.transform.parent = spriteRendererGO.transform;
        _upCoverRenderer = coverGO.AddComponent<SpriteRenderer>();
        _upCoverRenderer.sprite = _fullCoverSprite;
        _upCoverRenderer.color = new Color(1, 1, 1, 0.7f);
        coverGO.transform.localPosition = new Vector3(0, CombatGameManager.Instance.GridMap.CellSize / 2f - _displayHeight, -0.5f);
        coverGO.transform.localScale = 0.7f * Vector3.one;

        coverGO = new GameObject("downCover");
        coverGO.transform.parent = spriteRendererGO.transform;
        _downCoverRenderer = coverGO.AddComponent<SpriteRenderer>();
        _downCoverRenderer.sprite = _fullCoverSprite;
        _downCoverRenderer.color = new Color(1, 1, 1, 0.7f);
        coverGO.transform.localPosition = new Vector3(0, - CombatGameManager.Instance.GridMap.CellSize / 2f + _displayHeight, -0.5f);
        coverGO.transform.localScale = 0.7f * Vector3.one;

        coverGO = new GameObject("leftCover");
        coverGO.transform.parent = spriteRendererGO.transform;
        _leftCoverRenderer = coverGO.AddComponent<SpriteRenderer>();
        _leftCoverRenderer.sprite = _fullCoverSprite;
        _leftCoverRenderer.color = new Color(1, 1, 1, 0.7f);
        coverGO.transform.localPosition = new Vector3(- CombatGameManager.Instance.GridMap.CellSize / 2f + _displayHeight, 0, -0.5f);
        coverGO.transform.localScale = 0.7f * Vector3.one;
        coverGO.transform.rotation = Quaternion.Euler(0, 90, 0);

        coverGO = new GameObject("rightCover");
        coverGO.transform.parent = spriteRendererGO.transform;
        _rightCoverRenderer = coverGO.AddComponent<SpriteRenderer>();
        _rightCoverRenderer.sprite = _fullCoverSprite;
        _rightCoverRenderer.color = new Color(1, 1, 1, 0.7f);
        coverGO.transform.localPosition = new Vector3(CombatGameManager.Instance.GridMap.CellSize / 2f - _displayHeight, 0, -0.5f);
        coverGO.transform.localScale = 0.7f * Vector3.one;
        coverGO.transform.rotation = Quaternion.Euler(0, 90, 0);


        Vector3 p = CombatGameManager.Instance.GridMap.GridWorldCenter;
        _gridLinesRenderer.transform.position = new Vector3(p.x, _displayHeight, p.z);
        _gridLinesRenderer.transform.localScale = new Vector3(CombatGameManager.Instance.GridMap.GridWorldWidth, CombatGameManager.Instance.GridMap.GridWorldHeight, 1f);
        _gridLinesRenderer.sortingOrder = -1;
    }

    #region Mouse tile display

    public void HideMouseHoverTile()
    {
        _mouseHovertileSpriteRenderer.enabled = false;
        _upCoverRenderer.enabled    = false;
        _downCoverRenderer.enabled  = false;
        _leftCoverRenderer.enabled  = false;
        _rightCoverRenderer.enabled = false;
    }

    public void DisplayMouseHoverTileAt(Vector2Int coord)
    {
        _mouseHovertileSpriteRenderer.transform.position = CombatGameManager.Instance.GridMap.GridToWorld(coord, _displayHeight + 0.01f);
        _mouseHovertileSpriteRenderer.enabled = true;

        GridMap map = CombatGameManager.Instance.GridMap;
        Tile tile = map[coord];

        UpdateMouseCoordForGrid(coord);

        if (tile == null || tile.Cover != EnumCover.None)
        {
            _upCoverRenderer.enabled    = false;
            _downCoverRenderer.enabled  = false;
            _leftCoverRenderer.enabled  = false;
            _rightCoverRenderer.enabled = false;
            return;
        }

        tile = map[coord + new Vector2Int(0, 1)];
        if (tile == null || tile.Cover == EnumCover.None) _upCoverRenderer.enabled = false;
        else
        {
            _upCoverRenderer.enabled = true;
            _upCoverRenderer.sprite = tile.Cover == EnumCover.Half ? _halfCoverSprite : _fullCoverSprite;
        }

        tile = map[coord + new Vector2Int(0, -1)];
        if (tile == null || tile.Cover == EnumCover.None) _downCoverRenderer.enabled = false;
        else
        {
            _downCoverRenderer.enabled = true;
            _downCoverRenderer.sprite = tile.Cover == EnumCover.Half ? _halfCoverSprite : _fullCoverSprite;
        }

        tile = map[coord + new Vector2Int(-1, 0)];
        if (tile == null || tile.Cover == EnumCover.None) _leftCoverRenderer.enabled = false;
        else
        {
            _leftCoverRenderer.enabled = true;
            _leftCoverRenderer.sprite = tile.Cover == EnumCover.Half ? _halfCoverSprite : _fullCoverSprite;
        }

        tile = map[coord + new Vector2Int(1, 0)];
        if (tile == null || tile.Cover == EnumCover.None) _rightCoverRenderer.enabled = false;
        else
        {
            _rightCoverRenderer.enabled = true;
            _rightCoverRenderer.sprite = tile.Cover == EnumCover.Half ? _halfCoverSprite : _fullCoverSprite;
        }
    }

    #endregion

    #region Grid display

    public void DisplayGrid(bool display)
    {
        _gridLinesRenderer.material.SetInt("_DisplayAllGrid", display ? 1 : 0);
    }

    private void UpdateMouseCoordForGrid(Vector2Int coord)
    {
        if (coord != _previousMouseCoord)
        {
            Vector3 worldPos = CombatGameManager.Instance.GridMap.GridToWorld(coord, 0f);
            _gridLinesRenderer.material.SetVector("_MouseCoord", new Vector4(worldPos.x, worldPos.z));
            _previousMouseCoord = coord;
        }
    }

    #endregion

    #region Tile zone display

    public Color GetTileZoneColor(string name)
    {
        if (_tileZonesRenderers.ContainsKey(name))
        {
            return _tileZonesRenderers[name][0].material.GetColor("_Color");
        }

        return new Color(0, 0, 0, 0);
    }

    public bool CreateNewTileZone(TileZonePreset preset)
    {
        if (preset == null)
        {
            return false;
        }

        return CreateNewTileZone(preset.Name, preset.Tileset, preset.Color, preset.Order);
    }

    /// <summary>
    /// Create a new tile zone. The order defines the ordering order of each zone, higher values means on top of others
    /// </summary>
    public bool CreateNewTileZone(string name, Texture2D tileset, Color color, ushort order)
    {
        if (_tileZonesRenderers.ContainsKey(name)
            || name == null || name.Length == 0
            || tileset == null)
        {
            return false;
        }

        Vector3 gridOrigin = CombatGameManager.Instance.GridMap.GridOrigin;
        float cellSize = CombatGameManager.Instance.GridMap.CellSize;
        int widthChunkNumber = Mathf.CeilToInt((float)CombatGameManager.Instance.GridMap.GridTileWidth / _zonesChunkSizeInTiles);
        int heightChunkNumber = Mathf.CeilToInt((float)CombatGameManager.Instance.GridMap.GridTileHeight / _zonesChunkSizeInTiles);

        MeshRenderer[] zoneChunkRenderers = new MeshRenderer[widthChunkNumber * heightChunkNumber];
        Material zoneMaterial = new Material(_tileZoneRendererPrefab.sharedMaterial);
        zoneMaterial.SetVectorArray("_Coords", new Vector4[_zonesChunkSizeInTiles * _zonesChunkSizeInTiles]);
        zoneMaterial.SetFloatArray("_BlobIndices", new float[_zonesChunkSizeInTiles * _zonesChunkSizeInTiles]);
        zoneMaterial.SetTexture("_Tileset", tileset);
        zoneMaterial.SetColor("_Color", color);
        zoneMaterial.SetVector("_GridOrigin", gridOrigin);
        zoneMaterial.SetFloat("_CellSize", cellSize);

        // Compute the new orders for other tile zones
        if (order > maxOrder)
        {
            foreach (KeyValuePair<string, MeshRenderer[]> zoneKVP in _tileZonesRenderers)
            {
                foreach (MeshRenderer chunkRenderer in zoneKVP.Value)
                {
                    chunkRenderer.sortingOrder -= order - maxOrder;
                }
            }

            maxOrder = order;
        }

        for (int h = 0; h < heightChunkNumber; h++)
        {
            for (int w = 0; w < widthChunkNumber; w++)
            {
                int i = h * widthChunkNumber + w;
                zoneChunkRenderers[i] = Instantiate(_tileZoneRendererPrefab, transform);
                zoneChunkRenderers[i].material = zoneMaterial;
                zoneChunkRenderers[i].sortingOrder = order - maxOrder - 1;

                zoneChunkRenderers[i].transform.position = new Vector3
                (
                    gridOrigin.x + cellSize * (w * _zonesChunkSizeInTiles + _zonesChunkSizeInTiles / 2f),
                    _displayHeight,
                    gridOrigin.z + cellSize * (h * _zonesChunkSizeInTiles + _zonesChunkSizeInTiles / 2f)
                );
                zoneChunkRenderers[i].transform.localScale = new Vector3
                (
                    _zonesChunkSizeInTiles * CombatGameManager.Instance.GridMap.CellSize,
                    _zonesChunkSizeInTiles * CombatGameManager.Instance.GridMap.CellSize,
                    1f
                );
            }
        }

        _tileZonesRenderers.Add(name, zoneChunkRenderers);

        return true;
    }

    public void HideAllTileZones()
    {
        foreach (KeyValuePair<string, MeshRenderer[]> zoneKVP in _tileZonesRenderers)
        {
            foreach (MeshRenderer chunkRenderer in zoneKVP.Value)
            {
                chunkRenderer.enabled = false;
            }
        }
    }

    public bool HideTileZone(string name)
    {
        if (!_tileZonesRenderers.ContainsKey(name))
        {
            return false;
        }

        foreach (MeshRenderer chunkRenderer in _tileZonesRenderers[name])
        {
            chunkRenderer.enabled = false;
        }

        return true;
    }

    // Works with the blob tileset principle -> more here http://www.cr31.co.uk/stagecast/wang/blob.html
    public bool DisplayTileZone(string name, List<Tile> tiles, bool hideOthers = true)
    {
        if (hideOthers)
        {
            HideAllTileZones();
        }

        if (!_tileZonesRenderers.ContainsKey(name) || tiles.Count == 0)
        {
            return false;
        }

        int widthChunkNumber = Mathf.CeilToInt((float)CombatGameManager.Instance.GridMap.GridTileWidth / _zonesChunkSizeInTiles);
        List<Vector4>[] coordsVec4 = new List<Vector4>[_tileZonesRenderers[name].Length];
        List<float>[] blobIndices = new List<float>[_tileZonesRenderers[name].Length];
        for (int i = 0; i < tiles.Count; i++)
        {
            int chunkIndex = tiles[i].Coords.x / _zonesChunkSizeInTiles + widthChunkNumber * (tiles[i].Coords.y / _zonesChunkSizeInTiles);

            if (coordsVec4[chunkIndex] == null)
            {
                coordsVec4[chunkIndex] = new List<Vector4>();
                blobIndices[chunkIndex] = new List<float>();
            }
            coordsVec4[chunkIndex].Add(new Vector4(tiles[i].Coords.x, tiles[i].Coords.y));

            Tile[] neighbourTiles = CombatGameManager.Instance.GridMap.TileNeighbors(tiles[i].Coords);
            bool[] coordPresent = { false, false, false, false, false, false, false, false };
            for (int j = 0; j < neighbourTiles.Length; j++)
            {
                if (tiles.Contains(neighbourTiles[j]))
                {
                    coordPresent[NeighbourIndex(neighbourTiles[j].Coords - tiles[i].Coords)] = true;
                }
            }

            blobIndices[chunkIndex].Add(FindBlobIndexFromNeighbours(coordPresent));
        }

        for (int i = 0; i < _tileZonesRenderers[name].Length; i++)
        {
            if (coordsVec4[i] == null || coordsVec4[i].Count == 0)
            {
                _tileZonesRenderers[name][i].enabled = false;
            }
            else
            {
                _tileZonesRenderers[name][i].material.SetInt("_CoordsCount", coordsVec4[i].Count);
                _tileZonesRenderers[name][i].material.SetVectorArray("_Coords", coordsVec4[i].ToArray());
                _tileZonesRenderers[name][i].material.SetFloatArray("_BlobIndices", blobIndices[i].ToArray());
                _tileZonesRenderers[name][i].enabled = true;
            }
        }

        return true;
    }

    private int NeighbourIndex(Vector2Int coordDiff) => coordDiff switch
    {
        { x:  0, y:  1 } => 0,
        { x:  1, y:  1 } => 1,
        { x:  1, y:  0 } => 2,
        { x:  1, y: -1 } => 3,
        { x:  0, y: -1 } => 4,
        { x: -1, y: -1 } => 5,
        { x: -1, y:  0 } => 6,
        { x: -1, y:  1 } => 7,
        _                => 0,
    };

    private int FindBlobIndexFromNeighbours(bool[] neighbours)
    {
        int ret = 0;

        if (neighbours[0]) ret += 1;
        if (neighbours[1] && neighbours[0] && neighbours[2]) ret += 2;
        if (neighbours[2]) ret += 4;
        if (neighbours[3] && neighbours[2] && neighbours[4]) ret += 8;
        if (neighbours[4]) ret += 16;
        if (neighbours[5] && neighbours[4] && neighbours[6]) ret += 32;
        if (neighbours[6]) ret += 64;
        if (neighbours[7] && neighbours[6] && neighbours[0]) ret += 128;

        return ret;
    }

    #endregion
}
