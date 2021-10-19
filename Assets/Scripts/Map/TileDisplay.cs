using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class TileDisplay : MonoBehaviour
{
    [SerializeField] private float _displayHeight = 0.01f;
    [SerializeField] private BlobTilesetInfo[] _blobTilesets;
    [SerializeField] private Sprite _mouseHoverTileSprite;

    [System.Serializable]
    public struct BlobTilesetInfo
    {
        public TileZoneDisplayEnum TileZoneDisplay;
        public Texture2D BlobTileset;
        public int TilePixelSize;
        public Color Tint;

        public BlobTilesetInfo(Texture2D blob, Color t, TileZoneDisplayEnum d = TileZoneDisplayEnum.MoveZoneDisplay, int ts = 256)
        {
            BlobTileset = blob;
            TileZoneDisplay = d;
            TilePixelSize = ts;
            Tint = t;
        }
    }


    private List<SpriteRenderer> _spriteRenderersList = new List<SpriteRenderer>();
    private SpriteRenderer _mouseHovertileSpriteRenderer;

    private Dictionary<TileZoneDisplayEnum, Dictionary<int, Sprite>> _splitBlobTilesetsDictionary =
        new Dictionary<TileZoneDisplayEnum, Dictionary<int, Sprite>>(System.Enum.GetValues(typeof(TileZoneDisplayEnum)).Length);

    private Dictionary<TileZoneDisplayEnum, Color> _texturesTintDictionary = new Dictionary<TileZoneDisplayEnum, Color>();

    private void Start()
    {
        GameObject spriteRendererGO = new GameObject("MouseHoverTileSprite");
        spriteRendererGO.transform.parent = transform;
        _mouseHovertileSpriteRenderer = spriteRendererGO.AddComponent<SpriteRenderer>();
        _mouseHovertileSpriteRenderer.sprite = _mouseHoverTileSprite;
        _mouseHovertileSpriteRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Split blob tilesets
        for (int i = 0; i < _blobTilesets.Length; i++)
        {
            Assert.IsFalse
            (
                _splitBlobTilesetsDictionary.ContainsKey(_blobTilesets[i].TileZoneDisplay),
                "Each TileZoneDisplayEnum should appear exactly one time."
            );

            Dictionary<int, Sprite> _splitBlobTileset = new Dictionary<int, Sprite>(47);

            for (int j = 0; j < _blobTilesets[i].TilePixelSize; j++)
            {
                Sprite sprite = TextureCoordinateInBlobTileset(j, _blobTilesets[i]);
                if (sprite != null)
                {
                    _splitBlobTileset.Add(j, sprite);
                }
            }

            _splitBlobTilesetsDictionary.Add(_blobTilesets[i].TileZoneDisplay, _splitBlobTileset);
            _texturesTintDictionary.Add(_blobTilesets[i].TileZoneDisplay, _blobTilesets[i].Tint);
        }
    }

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
    public void DisplayTileZone(List<Tile> tiles, TileZoneDisplayEnum display)
    {
        int i = 0;
        for (; i < tiles.Count; i++)
        {
            Tile[] neighbourTiles = CombatGameManager.Instance.GridMap.MovementNeighbors(tiles[i].Coords);

            bool[] coordPresent = { false, false, false, false, false, false, false, false };
            for (int j = 0; j < neighbourTiles.Length; j++)
            {
                if (tiles.Contains(neighbourTiles[j]))
                {
                    coordPresent[NeighbourIndex(neighbourTiles[j].Coords - tiles[i].Coords)] = true;
                }
            }
            int textureIndex = FindBlobIndexFromNeighbours(coordPresent);

            if (i >= _spriteRenderersList.Count)
            {
                GameObject spriteRendererGO = new GameObject("MoveSprite");
                spriteRendererGO.transform.parent = transform;
                SpriteRenderer spriteRenderer = spriteRendererGO.AddComponent<SpriteRenderer>();
                spriteRenderer.transform.position = CombatGameManager.Instance.GridMap.GridToWorld(tiles[i].Coords, _displayHeight);
                spriteRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                spriteRenderer.sprite = _splitBlobTilesetsDictionary[display][textureIndex];
                spriteRenderer.color = _texturesTintDictionary[display];
                spriteRenderer.enabled = true;
                _spriteRenderersList.Add(spriteRenderer);
            }
            else
            {
                _spriteRenderersList[i].transform.position = CombatGameManager.Instance.GridMap.GridToWorld(tiles[i].Coords, _displayHeight);
                _spriteRenderersList[i].sprite = _splitBlobTilesetsDictionary[display][textureIndex];
                _spriteRenderersList[i].color = _texturesTintDictionary[display];
                _spriteRenderersList[i].enabled = true;
            }
        }

        for (; i < _spriteRenderersList.Count; i++)
        {
            _spriteRenderersList[i].enabled = false;
        }
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

    private Sprite TextureCoordinateInBlobTileset(int blobIndex, BlobTilesetInfo blobTilesetInfo) => blobIndex switch
    {
        0   => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(0, 0), blobTilesetInfo),
        4   => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(1, 0), blobTilesetInfo),
        71  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(2, 0), blobTilesetInfo),
        193 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(3, 0), blobTilesetInfo),
        7   => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(4, 0), blobTilesetInfo),
        199 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(5, 0), blobTilesetInfo),
        197 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(6, 0), blobTilesetInfo),
        64  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(7, 0), blobTilesetInfo),
        5   => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(0, 1), blobTilesetInfo),
        69  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(1, 1), blobTilesetInfo),
        93  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(2, 1), blobTilesetInfo),
        119 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(3, 1), blobTilesetInfo),
        223 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(4, 1), blobTilesetInfo),
        256 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(5, 1), blobTilesetInfo), // Not possible but keep it to fill coord hole
        245 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(6, 1), blobTilesetInfo),
        65  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(7, 1), blobTilesetInfo),
        23  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(0, 2), blobTilesetInfo),
        213 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(1, 2), blobTilesetInfo),
        81  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(2, 2), blobTilesetInfo),
        31  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(3, 2), blobTilesetInfo),
        253 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(4, 2), blobTilesetInfo),
        125 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(5, 2), blobTilesetInfo),
        113 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(6, 2), blobTilesetInfo),
        16  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(7, 2), blobTilesetInfo),
        29  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(0, 3), blobTilesetInfo),
        117 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(1, 3), blobTilesetInfo),
        85  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(2, 3), blobTilesetInfo),
        95  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(3, 3), blobTilesetInfo),
        247 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(4, 3), blobTilesetInfo),
        215 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(5, 3), blobTilesetInfo),
        209 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(6, 3), blobTilesetInfo),
        1   => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(7, 3), blobTilesetInfo),
        21  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(0, 4), blobTilesetInfo),
        84  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(1, 4), blobTilesetInfo),
        87  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(2, 4), blobTilesetInfo),
        221 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(3, 4), blobTilesetInfo),
        127 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(4, 4), blobTilesetInfo),
        255 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(5, 4), blobTilesetInfo),
        241 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(6, 4), blobTilesetInfo),
        17  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(7, 4), blobTilesetInfo),
        20  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(0, 5), blobTilesetInfo),
        68  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(1, 5), blobTilesetInfo),
        92  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(2, 5), blobTilesetInfo),
        112 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(3, 5), blobTilesetInfo),
        28  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(4, 5), blobTilesetInfo),
        124 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(5, 5), blobTilesetInfo),
        116 => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(6, 5), blobTilesetInfo),
        80  => CreateSpriteFromTextureCoordinateInBlobTileset(new Vector2Int(7, 5), blobTilesetInfo),
        _   => null,
    };

    private Sprite CreateSpriteFromTextureCoordinateInBlobTileset(Vector2Int coord, BlobTilesetInfo blobTilesetInfo)
    {
        // Create a _tilesPixelSize * _tilesPixelSize texture
        // at coordinate (coord.x * _tilesPixelSize, coord.y * _tilesPixelSize) in blob tileset
        Texture2D generatedTexture = new Texture2D(blobTilesetInfo.TilePixelSize, blobTilesetInfo.TilePixelSize);
        generatedTexture.SetPixels
        (
            blobTilesetInfo.BlobTileset.GetPixels
            (
                coord.x * blobTilesetInfo.TilePixelSize,
                coord.y * blobTilesetInfo.TilePixelSize,
                blobTilesetInfo.TilePixelSize,
                blobTilesetInfo.TilePixelSize
            )
        );
        generatedTexture.Apply();

        return Sprite.Create
        (
            generatedTexture,
            new Rect(0.0f, 0.0f, blobTilesetInfo.TilePixelSize, blobTilesetInfo.TilePixelSize),
            new Vector2(0.5f, 0.5f), blobTilesetInfo.TilePixelSize / CombatGameManager.Instance.GridMap.CellSize
        );
    }

    #endregion
}
