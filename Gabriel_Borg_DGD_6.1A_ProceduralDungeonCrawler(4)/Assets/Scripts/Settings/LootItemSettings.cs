using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "LootItemSettings", menuName = "PCG/LootItemSettings")]
public class LootItemSettings : ScriptableObject
{
    [Header("Loot Tiles")]
    public TileBase unopenedChestTile;
    public TileBase openedChestTile;
    public TileBase healthPickupTile;
    public TileBase doorKeyTile;

    [Header("Loot Placement Settings")]
    // Minimum loot tiles per room.
    public int minLootPerRoom = 1;

    // Maximum loot tiles per room.
    public int maxLootPerRoom = 2;

    // Chance that a room will contain loot.
    [Range(0f, 1f)]
    public float roomChanceToHaveLoot = 0.5f;
}
