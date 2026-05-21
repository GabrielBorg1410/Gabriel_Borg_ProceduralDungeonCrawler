using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
public class TrapPlacementGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TrapPlacementSettings settings;

    [Header("Dungeon Generator")]
    // Used to get the generated rooms from the dungeon.
    [SerializeField] private DungeonGenerator dungeonGenerator;

    [Header("Tilemap")]
    [SerializeField] private Tilemap trapTilemap;

    [Button("Generate Traps")]
    private void GenerateTrapsButton()
    {
        //Getting the function from the DungeonGenerator.cs
        GenerateTraps(dungeonGenerator.GetGeneratedRooms());
    }

    [Button("Clear Traps")]
    public void ClearTraps()
    {
        trapTilemap.ClearAllTiles();
    }

    //Generates traps inside the generated dungeon rooms.
    public void GenerateTraps(List<RectInt> rooms)
    {
        // Removes old traps first.
        ClearTraps();

        // Loops through every generated room.
        foreach (RectInt room in rooms)
        {
            // Random chance for this room to contain traps.
            if (Random.value > settings.roomChanceToHaveTraps)
                continue;

            // Random amount of traps in this room.
            int trapCount = Random.Range(settings.minTrapsPerRoom, settings.maxTrapsPerRoom + 1);

            for (int i = 0; i < trapCount; i++)
            {
                // Gets a random position inside the room.
                Vector3Int position = GetRandomPosition(room);

                // Gets a random trap tile.
                TileBase selectedTrap = GetRandomTrapTile();

                // Places the trap tile on the trap tilemap.
                trapTilemap.SetTile(position, selectedTrap);
            }
        }
    }

    //Gets a random position inside a room. +1 and -1 keep traps away from the room walls.
    private Vector3Int GetRandomPosition(RectInt room)
    {
        int x = Random.Range(room.xMin + 1, room.xMax - 1);
        int y = Random.Range(room.yMin + 1, room.yMax - 1);

        return new Vector3Int(x, y, 0);
    }

    //Randomly chooses one trap tile.
    private TileBase GetRandomTrapTile()
    {
        int randomNumber = Random.Range(0, 3);

        if (randomNumber == 0)
            return settings.spikeTrapTile;

        if (randomNumber == 1)
            return settings.poisonPoolTile;

        return settings.pressurePlateTile;
    }
}