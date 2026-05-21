using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

public class TerrainVariationGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TerrainVariationSettings settings;

    [Header("Dungeon Generator")]
    [SerializeField] private DungeonGenerator dungeonGenerator;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap decorationTilemap;
    [SerializeField] private Tilemap wallTilemap;


    [Button("Generate Terrain Variation")]
    private void GenerateTerrainVariation()
    {
        // Get all generated floor positions from the dungeon generator.
        // These positions will be used for both floor variation and decorations.
        HashSet<Vector2Int> floorPositions = dungeonGenerator.GetFloorPositions();

        // Clear old decorations before generating new ones.
        // This prevents decorations from stacking every time the button is pressed.
        decorationTilemap.ClearAllTiles();

        // Apply Perlin noise based floor variation
        // (stone, cracked, mossy, water, etc.)
        ApplyNoiseDrivenFloorVariation(floorPositions);

        // Place decorations such as bones, rubble, cobwebs and torches.
        PlaceProceduralDecorations(floorPositions);
    }

    [Button("Clear Terrain Variation")]
    private void ClearTerrainVariation()
    {
        floorTilemap.ClearAllTiles();
        decorationTilemap.ClearAllTiles();
    }

    public void ApplyNoiseDrivenFloorVariation(HashSet<Vector2Int> floorPositions)
    {
        // HashSet<Vector2Int> stores all floor positions from the dungeon.

        // Loop through every floor tile position.
        foreach (Vector2Int floorPosition in floorPositions)
        {
            // Convert Vector2Int to Vector3Int because Tilemaps use x, y, z coordinates.
            Vector3Int tilePosition = new Vector3Int(floorPosition.x, floorPosition.y, 0);

            // Choose which floor tile should appear at this position.
            TileBase selectedFloorTile = GetFloorTileFromNoise(tilePosition);

            // Paint the selected floor variation tile onto the floor tilemap.
            floorTilemap.SetTile(tilePosition, selectedFloorTile);
        }
    }

    private TileBase GetFloorTileFromNoise(Vector3Int position)
    {
        // Mathf.PerlinNoise creates smooth random values between 0 and 1.
        float noiseValue = Mathf.PerlinNoise(
            position.x * settings.NoiseScale,
            position.y * settings.NoiseScale
        );

        // If the noise value is small, use cracked tile.
        if (noiseValue < 0.25f)
        {
            return settings.CrackedTile;
        }
        // If the noise value is between 0.25 and 0.55, use stone tile.
        else if (noiseValue < 0.55f)
        {
            return settings.StoneTile;
        }
        // If the noise value is between 0.55 and 0.80, use mossy tile.
        else if (noiseValue < 0.80f)
        {
            return settings.MossyTile;
        }
        // If the noise value is 0.80 or higher, use water tile.
        else
        {
            return settings.WaterTile;
        }
    }

    private TileBase GetRandomFloorDecoration()
    {
        // Random.Range(0, 3) gives either 0, 1, or 2.
        int randomIndex = Random.Range(0, 3);

        // If 0, place bones.
        if (randomIndex == 0)
            return settings.BonesTile;

        // If  1, place crack decoration.
        else if (randomIndex == 1)
            return settings.CrackDecorationTile;

        // If 2, place cobweb.
        else
            return settings.CobwebTile;
    }

    private bool IsNearWall(Vector3Int position)
    {
        // Stores the four directions (up, down, left and right) around the current tile.
        Vector3Int[] directions =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        // Check every direction around this position.
        foreach (Vector3Int direction in directions)
        {
            // If the wall tilemap has a tile there, this position is near a wall.
            if (wallTilemap.HasTile(position + direction))
                return true;
        }

        // If no nearby wall was found, return false.
        return false;
    }

    private Vector3Int GetAdjacentWallPosition(Vector3Int position)
    {
        // Stores the four directions around the current tile.
        Vector3Int[] directions =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        // Check every nearby direction.
        foreach (Vector3Int direction in directions)
        {
            // Get the tile position.
            Vector3Int checkPosition = position + direction;

            // If this position has a wall tile, return that position.
            if (wallTilemap.HasTile(checkPosition))
                return checkPosition;
        }

        // If no wall tile was found, return (0, 0, 0)
        return Vector3Int.zero;
    }

    public void PlaceProceduralDecorations(HashSet<Vector2Int> floorPositions)
    {
        // Loop through every generated floor position from the dungeon.
        foreach (Vector2Int floorPosition in floorPositions)
        {
            // Convert the floor position into a Tilemap position.
            Vector3Int tilePosition = new Vector3Int(floorPosition.x, floorPosition.y, 0);

            // Random.value gives a number between 0 and 1.
            // If FloorDecorationChance is 0.2, that means around 20% chance.
            if (Random.value < settings.FloorDecorationChance)
            {
                // If this floor tile is near a wall, place rubble.
                if (IsNearWall(tilePosition))
                {
                    decorationTilemap.SetTile(tilePosition, settings.RubbleTile);
                }
                else
                {
                    // If it is not near a wall, place a random floor decoration.
                    decorationTilemap.SetTile(tilePosition,GetRandomFloorDecoration());
                }
            }

            // This checks if a torch should be placed on a nearby wall. It only works if the floor tile is near a wall.
            if (IsNearWall(tilePosition) &&
                Random.value < settings.WallDecorationChance)
            {
                // Find the nearby wall position.
                Vector3Int wallPosition = GetAdjacentWallPosition(tilePosition);

                // Only place the torch if a real wall position was found.
                if (wallPosition != Vector3Int.zero)
                {
                    decorationTilemap.SetTile(wallPosition,settings.TorchTile);
                }
            }
        }
    }
}