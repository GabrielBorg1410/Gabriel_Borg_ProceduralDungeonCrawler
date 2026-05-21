using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TrapPlacementSettings", menuName = "PCG/TrapPlacementSettings")]
public class TrapPlacementSettings : ScriptableObject
{
    [Header("Trap Tiles")]
    public TileBase spikeTrapTile;
    public TileBase poisonPoolTile;
    public TileBase pressurePlateTile;

    [Header("Trap Placement Settings")]
    // Minimum traps that can appear in one room.
    public int minTrapsPerRoom = 1;

    // Maximum traps that can appear in one room.
    public int maxTrapsPerRoom = 3;

    // Chance that a room will contain traps.
    [Range(0f, 1f)]
    public float roomChanceToHaveTraps = 0.6f;
}
