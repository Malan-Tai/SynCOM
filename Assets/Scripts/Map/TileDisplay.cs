using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class TileDisplay : MonoBehaviour
{
    [SerializeField] private float _displayHeight = 0.01f;
    [SerializeField] private Sprite _mouseHoverTileSprite;
    [SerializeField] private MeshRenderer _gridRenderer;

    [Header("Unit move zone")]
    [SerializeField] private Material _moveBlobMaterial;
    [SerializeField] private MeshRenderer _planeRenderer;

    // Sprite renderers to render tiles
    private SpriteRenderer _mouseHovertileSpriteRenderer;

    private void Start()
    {
        _planeRenderer.material = _moveBlobMaterial;

        GameObject spriteRendererGO = new GameObject("MouseHoverTileSprite");
        spriteRendererGO.transform.parent = transform;
        _mouseHovertileSpriteRenderer = spriteRendererGO.AddComponent<SpriteRenderer>();
        _mouseHovertileSpriteRenderer.sprite = _mouseHoverTileSprite;
        _mouseHovertileSpriteRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        DisplayGrid(true);
    }

    #region Grid display

    public void DisplayGrid(bool display)
    {
        if (display)
        {
            Vector3 p = CombatGameManager.Instance.GridMap.GridWorldCenter;
            _gridRenderer.transform.position = new Vector3(p.x, _displayHeight + 0.01f, p.z);
            _gridRenderer.transform.localScale = new Vector3(CombatGameManager.Instance.GridMap.GridWorldWidth, CombatGameManager.Instance.GridMap.GridWorldHeight, 1f);
            _gridRenderer.material.SetFloat("_CellSize", CombatGameManager.Instance.GridMap.CellSize);
        }

        _gridRenderer.enabled = display;
    }

    #endregion

    #region Tile display

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

    #region Tile zone display

    // Works with the blob tileset principle -> more here http://www.cr31.co.uk/stagecast/wang/blob.html
    public void UpdateTileZoneDisplay(List<Tile> tiles, TileZoneDisplayEnum display)
    {
        if (tiles.Count == 0)
        {
            _planeRenderer.enabled = false;
            return;
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
        _planeRenderer.transform.position = new Vector3(p.x, _displayHeight + 0.01f, p.z);
        _planeRenderer.transform.localScale = new Vector3(CombatGameManager.Instance.GridMap.GridWorldWidth, CombatGameManager.Instance.GridMap.GridWorldHeight, 1f);
        _moveBlobMaterial.SetInt("_GridWidthInTiles", CombatGameManager.Instance.GridMap.GridTileWidth);
        _moveBlobMaterial.SetInt("_GridHeightInTiles", CombatGameManager.Instance.GridMap.GridTileHeight);
        _moveBlobMaterial.SetVectorArray("_ReachableCoords", coordsVec4);
        _moveBlobMaterial.SetInt("_ReachableCoordsCount", tiles.Count);
        _moveBlobMaterial.SetFloatArray("_BlobIndices", blobIndices);
        _planeRenderer.enabled = true;
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

        if (ret == 0) ret = -1;

        return ret;
    }

    #endregion
}
