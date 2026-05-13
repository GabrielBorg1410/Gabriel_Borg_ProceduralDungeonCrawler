using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "FurniturePropsSettings", menuName = "PCG/FurniturePropsSettings")]
public class FurniturePropsSettings : ScriptableObject
{
    [Header("Furniture Tiles")]
    public TileBase barrelTile;
    public TileBase crateTile;
    public TileBase bookshelfTile;
    public TileBase tableTile;

    [Header("Placement Settings")]
    public int minPropsPerRoom = 1;
    public int maxPropsPerRoom = 4;
    public float roomChanceToHaveProps = 0.75f;
}
