using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class TileDisplay : MonoBehaviour
{
    [SerializeField] private float _displayHeight = 0.01f;
    [SerializeField] private Sprite _mouseHoverTileSprite;
    [SerializeField] private TileZonePreset[] _presets;
    [SerializeField] private MeshRenderer _tileZoneRendererPrefab;
    [SerializeField] private MeshRenderer _gridLinesRenderer;

    // Sprite renderers to render tiles
    private SpriteRenderer _mouseHovertileSpriteRenderer;

    private readonly Dictionary<string, MeshRenderer> _tileZonesRenderers = new Dictionary<string, MeshRenderer>();
    private ushort maxOrder = ushort.MinValue;

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
    }

    #region Mouse tile display

    public void HideMouseHoverTile()
    {
        _mouseHovertileSpriteRenderer.enabled = false;
    }

    public void DisplayMouseHoverTileAt(Vector2Int coord)
    {
        _mouseHovertileSpriteRenderer.transform.position = CombatGameManager.Instance.GridMap.GridToWorld(coord, _displayHeight + 0.01f);
        _mouseHovertileSpriteRenderer.enabled = true;
    }

    #endregion

    #region Grid display

    public void DisplayGrid(bool display)
    {
        if (display)
        {
            Vector3 p = CombatGameManager.Instance.GridMap.GridWorldCenter;
            _gridLinesRenderer.transform.position = new Vector3(p.x, _displayHeight, p.z);
            _gridLinesRenderer.transform.localScale = new Vector3(CombatGameManager.Instance.GridMap.GridWorldWidth, CombatGameManager.Instance.GridMap.GridWorldHeight, 1f);
        }

        _gridLinesRenderer.enabled = display;
    }

    #endregion

    #region Tile zone display

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

        MeshRenderer zoneRenderer = Instantiate(_tileZoneRendererPrefab, transform);
        Material zoneMaterial = new Material(_tileZoneRendererPrefab.sharedMaterial);
        zoneMaterial.SetVectorArray("_Coords", new Vector4[1000]);
        zoneMaterial.SetFloatArray("_BlobIndices", new float[1000]);
        zoneMaterial.SetTexture("_Tileset", tileset);
        zoneMaterial.SetColor("_Color", color);
        zoneRenderer.material = zoneMaterial;

        if (order > maxOrder)
        {
            foreach (KeyValuePair<string, MeshRenderer> zoneKVP in _tileZonesRenderers)
            {
                _tileZonesRenderers[zoneKVP.Key].sortingOrder -= order - maxOrder;
            }

            maxOrder = order;
            zoneRenderer.sortingOrder = -1;
        }
        else
        {
            zoneRenderer.sortingOrder = order - maxOrder - 1;
        }

        _tileZonesRenderers.Add(name, zoneRenderer);

        return true;
    }

    public void HideAllTileZones()
    {
        foreach (KeyValuePair<string, MeshRenderer> zoneKVP in _tileZonesRenderers)
        {
            _tileZonesRenderers[zoneKVP.Key].enabled = false;
        }
    }

    public bool HideTileZone(string name)
    {
        if (!_tileZonesRenderers.ContainsKey(name))
        {
            return false;
        }

        _tileZonesRenderers[name].enabled = false;
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

        Vector4[] coordsVec4 = new Vector4[tiles.Count];
        float[] blobIndices = new float[tiles.Count];
        for (int i = 0; i < tiles.Count; i++)
        {
            coordsVec4[i] = new Vector4(tiles[i].Coords.x, tiles[i].Coords.y);

            Tile[] neighbourTiles = CombatGameManager.Instance.GridMap.TileNeighbors(tiles[i].Coords);

            bool[] coordPresent = { false, false, false, false, false, false, false, false };
            for (int j = 0; j < neighbourTiles.Length; j++)
            {
                if (tiles.Contains(neighbourTiles[j]))
                {
                    coordPresent[NeighbourIndex(neighbourTiles[j].Coords - tiles[i].Coords)] = true;
                }
            }

            blobIndices[i] = FindBlobIndexFromNeighbours(coordPresent);
        }

        Vector3 p = CombatGameManager.Instance.GridMap.GridWorldCenter;
        _tileZonesRenderers[name].transform.position = new Vector3(p.x, _displayHeight, p.z);
        _tileZonesRenderers[name].transform.localScale = new Vector3(CombatGameManager.Instance.GridMap.GridWorldWidth, CombatGameManager.Instance.GridMap.GridWorldHeight, 1f);
        _tileZonesRenderers[name].material.SetInt("_GridWidthInTiles", CombatGameManager.Instance.GridMap.GridTileWidth);
        _tileZonesRenderers[name].material.SetInt("_GridHeightInTiles", CombatGameManager.Instance.GridMap.GridTileHeight);
        _tileZonesRenderers[name].material.SetInt("_CoordsCount", tiles.Count);
        _tileZonesRenderers[name].material.SetVectorArray("_Coords", coordsVec4);
        _tileZonesRenderers[name].material.SetFloatArray("_BlobIndices", blobIndices);
        _tileZonesRenderers[name].enabled = true;

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
