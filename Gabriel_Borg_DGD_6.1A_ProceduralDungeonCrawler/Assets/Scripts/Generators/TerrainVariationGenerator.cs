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
        ApplyNoiseDrivenFloorVariation(dungeonGenerator.GetFloorPositions());
        PlaceProceduralDecorations(dungeonGenerator.GetFloorPositions());

    }

    [Button("Clear Terrain Variation")]
    private void ClearTerrainVariation()
    {
        floorTilemap.ClearAllTiles();
    }

    /*Applies Perlin noise floor variation to the dungeon floor.*/
    public void ApplyNoiseDrivenFloorVariation(HashSet<Vector2Int> floorPositions)
    {
        foreach (Vector2Int floorPosition in floorPositions)
        {
            /*Converts Vector2Int positions into Vector3Int positions because Tilemaps use Vector3Int coordinates.*/
            Vector3Int tilePosition = new Vector3Int(
                floorPosition.x,
                floorPosition.y,
                0
            );

            /*Gets a floor tile variation using Perlin noise.*/
            TileBase selectedFloorTile =
                GetFloorTileFromNoise(tilePosition);

            /*Paints the selected floor tile onto the Tilemap.*/
            floorTilemap.SetTile(tilePosition, selectedFloorTile);
        }
    }

    /*Uses Perlin noise to determine which floor tile should appear.*/
    private TileBase GetFloorTileFromNoise(Vector3Int position)
    {
        float noiseValue = Mathf.PerlinNoise(
            position.x * settings.NoiseScale,
            position.y * settings.NoiseScale
        );

        if (noiseValue < 0.25f)
        {
            return settings.CrackedTile;
        }
        else if (noiseValue < 0.55f)
        {
            return settings.StoneTile;
        }
        else if (noiseValue < 0.80f)
        {
            return settings.MossyTile;
        }
        else
        {
            return settings.WaterTile;
        }
    }

/*
This function randomly selects a floor decoration tile.

A random number is generated to choose between:
- bones
- cracks
- cobwebs

This creates visual variety across the dungeon floor.
*/
    private TileBase GetRandomFloorDecoration()
    {
        //Generate a random number between 0 and 2.
        int randomIndex = Random.Range(0, 3);

        //Return a decoration tile based on the random number.
        if (randomIndex == 0)
            return settings.BonesTile;
        else if (randomIndex == 1)
            return settings.CrackDecorationTile;
        else
            return settings.CobwebTile;
    }

    //Checking if a floor tile is close to a wall tile.
    private bool IsNearWall(Vector3Int position)
    {
        /*Storing the four directions around the current tile: up, down, left and right.*/
        Vector3Int[] directions =
        {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

        /*Checking every nearby direction around the tile.*/
        foreach (Vector3Int direction in directions)
        {
            /*If a wall tile exists in one of the nearby positions, return true.*/
            if (wallTilemap.HasTile(position + direction))
                return true;
        }

        //No nearby wall was found.
        return false;
    }

    /*Finding the position of a nearby wall tile for placing wall decorations such as torches onto valid wall positions.*/
    private Vector3Int GetAdjacentWallPosition(Vector3Int position)
    {
        /*Storing the four directions around the current tile:up, down, left and right.*/
        Vector3Int[] directions =
        {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

        /*Checking each nearby direction for a wall tile.*/
        foreach (Vector3Int direction in directions)
        {
            //Getting the nearby tile position.
            Vector3Int checkPosition = position + direction;

            /*If a wall tile exists at this position, return that wall position.*/
            if (wallTilemap.HasTile(checkPosition))
                return checkPosition;
        }

        /*If no wall tile was found, return an empty position.
        */
        return Vector3Int.zero;
    }

    //Places procedural decorations across the dungeon.Decorations are placed using random probability rules.
    public void PlaceProceduralDecorations(HashSet<Vector2Int> floorPositions)
    {
        //Looping through every floor tile position.
        foreach (Vector2Int floorPosition in floorPositions)
        {
            //Converting the floor position into a Tilemap position.
            Vector3Int tilePosition = new Vector3Int(
                floorPosition.x,
                floorPosition.y,
                0
            );

            //Place floor decorations using probability.
            if (Random.value < settings.FloorDecorationChance)
            {
                //Place rubble near walls.
                if (IsNearWall(tilePosition))
                {
                    decorationTilemap.SetTile(
                        tilePosition,
                        settings.RubbleTile
                    );
                }
                else
                {
                    //Place random floor decorations.
                    decorationTilemap.SetTile(
                        tilePosition,
                        GetRandomFloorDecoration()
                    );
                }
            }

            //Place torches on nearby walls.
            if (IsNearWall(tilePosition) &&
                Random.value < settings.WallDecorationChance)
            {
                Vector3Int wallPosition =
                    GetAdjacentWallPosition(tilePosition);

                //Only place the torch if a valid wall exists.
                if (wallPosition != Vector3Int.zero)
                {
                    decorationTilemap.SetTile(
                        wallPosition,
                        settings.TorchTile
                    );
                }
            }
        }
    }
}