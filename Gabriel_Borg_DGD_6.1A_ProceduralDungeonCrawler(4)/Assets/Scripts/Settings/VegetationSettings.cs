using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "VegetationSettings", menuName = "PCG/VegetationSettings")]
public class VegetationSettings : ScriptableObject
{
    [Header("Vegetation Tiles")]
    public TileBase mossTile;
    public TileBase mushroomTile;
    public TileBase vineTile;
    public TileBase rootTile;

    [Header("Moss, Fungi and Vine Spawn Chances")]
    [Range(0f, 1f)]
    public float mossChance = 0.12f;

    [Range(0f, 1f)]
    public float mushroomChance = 0.05f;

    [Range(0f, 1f)]
    public float vineChance = 0.08f;
    
    //Settings for the maximum length of the tree root
    [Header("Tree Root Settings")]
    public int maxRootLength = 4;
}
