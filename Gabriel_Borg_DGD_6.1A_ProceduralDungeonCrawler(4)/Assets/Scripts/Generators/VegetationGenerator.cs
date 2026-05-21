using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VegetationGenerator : MonoBehaviour
{
    [Header("Vegetation Settings")]
    [SerializeField] private VegetationSettings settings;

    [Header("Dungeon Generator")]
    [SerializeField] private DungeonGenerator dungeonGenerator;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap vegetationTilemap;

    [Button("Generate Vegetation")]
    private void GenerateVegetationButton()
    {
        GenerateVegetation();
    }

    [Button("Clear Vegetation")]
    public void ClearVegetation()
    {
        vegetationTilemap.ClearAllTiles();
    }

    //Generates moss, mushrooms, vines and roots.
    public void GenerateVegetation()
    {
        // Removes old vegetation.
        vegetationTilemap.ClearAllTiles();

        // Gets all floor positions directly from the Tilemap.
        HashSet<Vector2Int> floorPositions = GetFloorPositionsFromTilemap();


        foreach (Vector2Int position in floorPositions)
        {
            // Converts Vector2Int to Vector3Int for Tilemap usage.
            Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);

            // Checks if the tile is near a wall or corner.
            bool nearWall = IsNearWall(tilePosition);
            bool corner = IsCorner(tilePosition);

            // Places moss near walls.
            if (nearWall && Random.value < settings.mossChance)
            {
                vegetationTilemap.SetTile(tilePosition, settings.mossTile);
            }

            // Places mushrooms in corners.
            if (corner && Random.value < settings.mushroomChance)
            {
                vegetationTilemap.SetTile(tilePosition, settings.mushroomTile);
            }
        }
        // Generates vines and roots.
        GenerateVinesAndRoots(floorPositions);
    }

    //Gets all floor positions directly from the floor Tilemap.
    private HashSet<Vector2Int> GetFloorPositionsFromTilemap()
    {
        // Stores all detected floor positions.
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        // Gets the total Tilemap bounds.
        BoundsInt bounds = floorTilemap.cellBounds;

        // Loops through every tile position inside the Tilemap.
        foreach (Vector3Int cellPosition in bounds.allPositionsWithin)
        {
            // Checks if there is a floor tile at this position.
            if (floorTilemap.HasTile(cellPosition))
            {
                // Stores valid floor positions.
                positions.Add(new Vector2Int(cellPosition.x, cellPosition.y));
            }
        }

        return positions;
    }

    //Generates vines below walls and roots downward.
    private void GenerateVinesAndRoots(HashSet<Vector2Int> floorPositions)
    {
        foreach (Vector2Int floorPosition in floorPositions)
        {
            Vector3Int tilePosition = new Vector3Int(floorPosition.x, floorPosition.y, 0);

            Vector3Int wallAbove = tilePosition + Vector3Int.up;

            // Checks if there is a wall above the floor tile.
            if (wallTilemap.HasTile(wallAbove) &&
                Random.value < settings.vineChance)
            {
                // Places vine tiles.
                vegetationTilemap.SetTile(tilePosition, settings.vineTile);

                // Creates a random root length.
                int rootLength = Random.Range(1, settings.maxRootLength + 1);

                for (int i = 1; i <= rootLength; i++)
                {
                    Vector3Int rootPosition = tilePosition + Vector3Int.down * i;

                    // Places roots on valid floor tiles.
                    if (floorTilemap.HasTile(rootPosition))
                    {
                        vegetationTilemap.SetTile(rootPosition, settings.rootTile);
                    }
                }
            }
        }
    }

    //Checks if a floor tile is beside a wall.
    private bool IsNearWall(Vector3Int position)
    {
        Vector3Int[] directions =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int direction in directions)
        {
            if (wallTilemap.HasTile(position + direction))
            {
                return true;
            }
        }
        return false;
    }

    //Checks if a floor tile is inside a corner.
    private bool IsCorner(Vector3Int position)
    {
        bool wallUp = wallTilemap.HasTile(position + Vector3Int.up);
        bool wallDown = wallTilemap.HasTile(position + Vector3Int.down);
        bool wallLeft = wallTilemap.HasTile(position + Vector3Int.left);
        bool wallRight = wallTilemap.HasTile(position + Vector3Int.right);

        return (wallUp && wallLeft) ||
               (wallUp && wallRight) ||
               (wallDown && wallLeft) ||
               (wallDown && wallRight);
    }
}