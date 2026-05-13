using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

public class FurniturePropsGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private FurniturePropsSettings settings;

    [Header("Dungeon Generator")]
    // Used to get the generated rooms from the dungeon.
    [SerializeField] private DungeonGenerator dungeonGenerator;

    [Header("Tilemap")]
    [SerializeField] private Tilemap furniturePropsTilemap;

    //Bbutton for generating furniture props.
    [Button("Generate Furniture Props")]
    private void GenerateFurnitureButton()
    {
        //Getting the function from the DungeonGenerator.cs
        GenerateFurniture(dungeonGenerator.GetGeneratedRooms());
    }

    //Clears all furniture props from the tilemap.
    [Button("Clear Furniture Props")]
    public void ClearFurniture()
    {
        furniturePropsTilemap.ClearAllTiles();
    }

    //Generates furniture inside the generated dungeon rooms.
    public void GenerateFurniture(List<RectInt> rooms)
    {
        // Removes old furniture first.
        ClearFurniture();

        // Loops through each generated room.
        foreach (RectInt room in rooms)
        {
            // Random chance for this room to contain furniture.
            if (Random.value > settings.roomChanceToHaveProps)
                continue;

            // Random amount of props in this room.
            int propCount = Random.Range(settings.minPropsPerRoom, settings.maxPropsPerRoom + 1);

            for (int i = 0; i < propCount; i++)
            {
                // Gets a random position inside the room.
                Vector3Int position = GetRandomPosition(room);

                // Gets a random furniture tile.
                TileBase selectedTile = GetRandomPropTile();

                // Places the furniture tile.
                furniturePropsTilemap.SetTile(position, selectedTile);
            }
        }
    }

    //Gets a random tile position inside a room. +1 and -1 keep furniture away from the walls.
    private Vector3Int GetRandomPosition(RectInt room)
    {
        int x = Random.Range(room.xMin + 1, room.xMax - 1);
        int y = Random.Range(room.yMin + 1, room.yMax - 1);

        return new Vector3Int(x, y, 0);
    }

    //Randomly chooses one furniture tile.
    private TileBase GetRandomPropTile()
    {
        int randomNumber = Random.Range(0, 4);

        if (randomNumber == 0)
            return settings.barrelTile;

        if (randomNumber == 1)
            return settings.crateTile;

        if (randomNumber == 2)
            return settings.tableTile;

        return settings.bookshelfTile;
    }
}