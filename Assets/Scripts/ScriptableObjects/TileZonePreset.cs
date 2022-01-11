using UnityEngine;

[CreateAssetMenu(fileName = "New Tile zone preset", menuName = "Tile zone")]
public class TileZonePreset : ScriptableObject
{
    public string Name;
    public Texture2D Tileset;
    public Color Color = new Color(1, 1, 1, 1);
    public ushort Order = 0;
}
