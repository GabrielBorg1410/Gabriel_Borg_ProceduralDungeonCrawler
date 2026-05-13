using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

public class LootItemGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LootItemSettings settings;

    [Header("Dungeon Generator")]
    // Used to get the generated rooms from the dungeon.
    [SerializeField] private DungeonGenerator dungeonGenerator;

    [Header("Tilemap")]
    [SerializeField] private Tilemap lootTilemap;

    [Button("Generate Loot Items")]
    private void GenerateLootButton()
    {
        //Getting the function from the DungeonGenerator.cs
        GenerateLoot(dungeonGenerator.GetGeneratedRooms());
    }

    [Button("Clear Loot Items")]
    public void ClearLoot()
    {
        lootTilemap.ClearAllTiles();
    }

    //Generates loot inside the generated dungeon rooms.
    public void GenerateLoot(List<RectInt> rooms)
    {
        // Removes old loot first.
        ClearLoot();

        // Stops if no rooms exist.
        if (rooms.Count == 0)
            return;

        // Places normal loot in rooms.
        foreach (RectInt room in rooms)
        {
            // Random chance for this room to contain loot.
            if (Random.value > settings.roomChanceToHaveLoot)
                continue;

            // Random amount of loot in this room.
            int lootCount = Random.Range(settings.minLootPerRoom, settings.maxLootPerRoom + 1);

            for (int i = 0; i < lootCount; i++)
            {
                // Gets a random position inside the room.
                Vector3Int position = GetRandomPosition(room);

                // Randomly chooses between chest and health pickup.
                TileBase selectedLoot = GetRandomLootTile();

                // Places the loot tile.
                lootTilemap.SetTile(position, selectedLoot);
            }
        }
        // Places one door key somewhere in the dungeon.
        PlaceDoorKey(rooms);
    }

    //Places exactly one door key in a random room.
    private void PlaceDoorKey(List<RectInt> rooms)
    {
        // Chooses one random room.
        RectInt keyRoom = rooms[Random.Range(0, rooms.Count)];

        // Gets a random position inside that room.
        Vector3Int keyPosition = GetRandomPosition(keyRoom);

        // Places the door key tile.
        lootTilemap.SetTile(keyPosition, settings.doorKeyTile);
    }

    //Gets a random position inside a room. +1 and -1 keep loot away from walls.
    private Vector3Int GetRandomPosition(RectInt room)
    {
        int x = Random.Range(room.xMin + 1, room.xMax - 1);
        int y = Random.Range(room.yMin + 1, room.yMax - 1);

        return new Vector3Int(x, y, 0);
    }

    //Randomly chooses one normal loot tile.
    private TileBase GetRandomLootTile()
    {
        int randomNumber = Random.Range(0, 2);

        if (randomNumber == 0)
            return settings.unopenedChestTile;

        return settings.healthPickupTile;
    }
}