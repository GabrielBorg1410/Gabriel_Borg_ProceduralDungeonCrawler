using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [SerializeField] private DungeonSettings settings;


    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;

    [SerializeField] private Tilemap wallTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallHorizontalTile;
    [SerializeField] private TileBase wallVerticalTile;
    [SerializeField] private TileBase exitTile;

    [Header("Spawn the Player")]
    [SerializeField] private GameObject playerPrefab;

    private GameObject spawnedPlayer;
    // Stores the spawned player so it can be deleted when resetting the dungeon.

    private BSPNode rootPartition;
    // The first BSP partition. It represents the whole dungeon area.

    private List<BSPNode> finalPartitions = new List<BSPNode>();
    // Stores the final smaller partitions after BSP splitting.

    private List<RectInt> generatedRooms = new List<RectInt>();
    // Stores all rooms. RectInt means a rectangle with x, y, width, and height.

    private List<Vector2Int> corridorPositions = new List<Vector2Int>();
    // Stores every tile position used for corridors.

    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
    // Stores all floor positions. HashSet prevents duplicate positions.

    [Button("Generate Dungeon")]
    private void GenerateDungeonButton()
    {
        GenerateDungeon(); // Runs the full dungeon generation process.
    }

    [Button("Reset Dungeon")]
    private void ResetDungeonButton()
    {
        ClearDungeon(); // Clears the dungeon from the scene.
        Debug.Log("Dungeon reset.");
    }

    private void GenerateDungeon()
    {
        // Stops generation if something important is missing in the Inspector.
        if (!CanGenerateDungeon())
            return;

        Debug.Log("Generating dungeon...");

        //Functions used to generate the dungeon

        SetSeed(); // Sets random or fixed seed.
        ClearDungeon(); // Clears old dungeon before generating a new one.

        CreateRootPartition(); // Creates the whole dungeon area as one big rectangle.
        SplitPartition(rootPartition, 0); // Splits the dungeon into smaller sections.

        CreateRoomsInsidePartitions(); // Creates rooms inside the final partitions.
        CreateCorridorsBetweenRooms(); // Connects rooms together.

        PaintFloorTiles(); // Paints rooms and corridors on the floor tilemap.
        PaintWallTiles(); // Paints walls around the floor tiles.

        PlacePlayerAndExit(); // Spawns the player in the first room of the dungeon and the exit in the last room of the dungeon.

        Debug.Log("Rooms created: " + generatedRooms.Count);
        Debug.Log("Corridor tiles created: " + corridorPositions.Count);
    }

    public HashSet<Vector2Int> GetFloorPositions()
    {
        // Gives the TerrainVariationGenerator access to the generated floor positions.
        return floorPositions;
    }

    private bool CanGenerateDungeon()
    {
        // Checks if DungeonSettings is assigned.
        if (settings == null)
        {
            Debug.LogError("Dungeon settings are missing.");
            return false;
        }

        // Checks if tilemaps are assigned.
        if (floorTilemap == null || wallTilemap == null)
        {
            Debug.LogError("Floor or wall tilemap is missing.");
            return false;
        }

        // Checks if all tile assets are assigned.
        if (floorTile == null || wallHorizontalTile == null || wallVerticalTile == null || exitTile == null)
        {
            Debug.LogError("One or more tiles are missing.");
            return false;
        }

        // Checks if the player prefab is assigned.
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing.");
            return false;
        }

        return true; // Everything is assigned correctly.
    }

    private void ClearDungeon()
    {
        // Removes all floor tiles.
        if (floorTilemap != null)
            floorTilemap.ClearAllTiles();

        // Removes all wall tiles.
        if (wallTilemap != null)
            wallTilemap.ClearAllTiles();

        // Clears stored dungeon data.
        finalPartitions.Clear();
        generatedRooms.Clear();
        corridorPositions.Clear();
        floorPositions.Clear();

        // Deletes the old spawned player.
        if (spawnedPlayer != null)
            DestroyImmediate(spawnedPlayer);
    }

    private void SetSeed()
    {
        if (settings.useRandomSeed)
        {
            // Uses the current millisecond to create different dungeon layouts.
            int randomSeed = System.DateTime.Now.Millisecond;
            Random.InitState(randomSeed);
            Debug.Log("Using random seed: " + randomSeed);
        }
        else
        {
            // Uses a fixed seed so the same dungeon can be generated again.
            Random.InitState(settings.seed);
            Debug.Log("Using fixed seed: " + settings.seed);
        }
    }

    private void CreateRootPartition()
    {
        // RectInt stores a rectangle using x, y, width, and height.
        RectInt dungeonArea = new RectInt(0, 0, settings.dungeonWidth, settings.dungeonHeight);

        // Creates the root BSP node from the whole dungeon area.
        rootPartition = new BSPNode(dungeonArea);
    }

    private void SplitPartition(BSPNode partition, int depth)
    {
        // Stops if the partition does not exist.
        if (partition == null)
            return;

        // If max depth is reached, this partition becomes a final partition.
        if (depth >= settings.maxDepth)
        {
            finalPartitions.Add(partition);
            return;
        }

        // Decides if the partition should be split horizontally or vertically.
        bool splitHorizontally = ShouldSplitHorizontally(partition);

        if (splitHorizontally)
            SplitPartitionHorizontally(partition, depth);
        else
            SplitPartitionVertically(partition, depth);
    }

    private bool ShouldSplitHorizontally(BSPNode partition)
    {
        // If the area is wider, split vertically into left and right.
        if (partition.area.width > partition.area.height)
            return false;

        // If the area is taller, split horizontally into top and bottom.
        if (partition.area.height > partition.area.width)
            return true;

        // If both are similar, choose randomly.
        return Random.value > 0.5f;
    }

    private void SplitPartitionHorizontally(BSPNode partition, int depth)
    {
        // Stops splitting if the partition is too short.
        if (partition.area.height < settings.minPartitionSize * 2)
        {
            finalPartitions.Add(partition);
            return;
        }

        // Chooses a random Y position to split the partition.
        int splitY = Random.Range(settings.minPartitionSize, partition.area.height - settings.minPartitionSize);

        // Creates the bottom section.
        partition.left = new BSPNode(
            new RectInt(partition.area.x, partition.area.y, partition.area.width, splitY)
        );

        // Creates the top section.
        partition.right = new BSPNode(
            new RectInt(partition.area.x, partition.area.y + splitY, partition.area.width, partition.area.height - splitY)
        );

        // Recursively keeps splitting both new sections.
        SplitPartition(partition.left, depth + 1);
        SplitPartition(partition.right, depth + 1);
    }

    private void SplitPartitionVertically(BSPNode partition, int depth)
    {
        // Stops splitting if the partition is too narrow.
        if (partition.area.width < settings.minPartitionSize * 2)
        {
            finalPartitions.Add(partition);
            return;
        }

        // Chooses a random X position to split the partition.
        int splitX = Random.Range(settings.minPartitionSize, partition.area.width - settings.minPartitionSize
        );

        // Creates the left section.
        partition.left = new BSPNode(
            new RectInt(partition.area.x, partition.area.y, splitX, partition.area.height)
        );

        // Creates the right section.
        partition.right = new BSPNode(
            new RectInt(partition.area.x + splitX, partition.area.y, partition.area.width - splitX, partition.area.height)
        );

        // Recursively keeps splitting both new sections.
        SplitPartition(partition.left, depth + 1);
        SplitPartition(partition.right, depth + 1);
    }

    private void CreateRoomsInsidePartitions()
    {
        // Loops through every final BSP partition.
        foreach (BSPNode partition in finalPartitions)
        {
            // Calculates the biggest room size allowed after padding.
            int maxRoomWidth = partition.area.width - settings.roomPadding * 2;
            int maxRoomHeight = partition.area.height - settings.roomPadding * 2;

            // Skips this partition if it is too small for a room.
            if (maxRoomWidth < settings.minRoomSize || maxRoomHeight < settings.minRoomSize)
                continue;

            // Chooses random room width.
            // Mathf.Min prevents the room from becoming bigger than the partition.
            int roomWidth = Random.Range(
                settings.minRoomSize,
                Mathf.Min(settings.maxRoomSize, maxRoomWidth) + 1
            );

            // Chooses random room height.
            int roomHeight = Random.Range(
                settings.minRoomSize,
                Mathf.Min(settings.maxRoomSize, maxRoomHeight) + 1
            );

            // Chooses random X position inside the partition.
            int roomX = Random.Range(
                partition.area.x + settings.roomPadding,
                partition.area.xMax - roomWidth - settings.roomPadding + 1
            );

            // Chooses random Y position inside the partition.
            int roomY = Random.Range(
                partition.area.y + settings.roomPadding,
                partition.area.yMax - roomHeight - settings.roomPadding + 1
            );

            // Creates the room rectangle.
            RectInt room = new RectInt(roomX, roomY, roomWidth, roomHeight);

            // Stores the room.
            partition.room = room;
            generatedRooms.Add(room);
        }
    }

    private void CreateCorridorsBetweenRooms()
    {
        // If there are less than two rooms, corridors are not needed.
        if (generatedRooms.Count <= 1)
            return;

        // Connects each room to the next room.
        for (int i = 0; i < generatedRooms.Count - 1; i++)
        {
            Vector2Int firstRoomCenter = GetRoomCenter(generatedRooms[i]);
            Vector2Int secondRoomCenter = GetRoomCenter(generatedRooms[i + 1]);

            CreateLCorridor(firstRoomCenter, secondRoomCenter);
        }
    }

    private Vector2Int GetRoomCenter(RectInt room)
    {
        // Finds the middle X and Y position of a room.
        int centerX = room.x + room.width / 2;
        int centerY = room.y + room.height / 2;

        return new Vector2Int(centerX, centerY);
    }

    private void CreateLCorridor(Vector2Int start, Vector2Int end)
    {
        // Randomly chooses whether the corridor goes sideways first or up/down first.
        bool horizontalFirst = Random.value > 0.5f;

        if (horizontalFirst)
        {
            CreateHorizontalCorridor(start.x, end.x, start.y);
            CreateVerticalCorridor(start.y, end.y, end.x);
        }
        else
        {
            CreateVerticalCorridor(start.y, end.y, start.x);
            CreateHorizontalCorridor(start.x, end.x, end.y);
        }
    }

    private void CreateHorizontalCorridor(int startX, int endX, int y)
    {
        // Finds the smaller and larger X values so the loop works in both directions.
        int minX = Mathf.Min(startX, endX);
        int maxX = Mathf.Max(startX, endX);

        // Adds every tile position between minX and maxX.
        for (int x = minX; x <= maxX; x++)
        {
            corridorPositions.Add(new Vector2Int(x, y));
        }
    }

    private void CreateVerticalCorridor(int startY, int endY, int x)
    {
        // Finds the smaller and larger Y values so the loop works in both directions.
        int minY = Mathf.Min(startY, endY);
        int maxY = Mathf.Max(startY, endY);

        // Adds every tile position between minY and maxY.
        for (int y = minY; y <= maxY; y++)
        {
            corridorPositions.Add(new Vector2Int(x, y));
        }
    }

    private void PaintFloorTiles()
    {
        // Paints all room floors.
        foreach (RectInt room in generatedRooms)
        {
            PaintRoomFloor(room);
        }

        // Paints all corridor floors.
        foreach (Vector2Int corridorPosition in corridorPositions)
        {
            PaintFloorTile(corridorPosition);
        }
    }

    private void PaintRoomFloor(RectInt room)
    {
        // Loops through every tile inside the room rectangle.
        for (int x = room.x; x < room.xMax; x++)
        {
            for (int y = room.y; y < room.yMax; y++)
            {
                PaintFloorTile(new Vector2Int(x, y));
            }
        }
    }

    private void PaintFloorTile(Vector2Int position)
    {
        // Stores this as a floor position.
        floorPositions.Add(position);

        // Converts Vector2Int to Vector3Int because Tilemaps use Vector3Int.
        Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);

        // Paints the floor tile.
        floorTilemap.SetTile(tilePosition, floorTile);
    }

    private void PaintWallTiles()
    {
        // For every floor tile, try placing walls around it.
        foreach (Vector2Int floorPosition in floorPositions)
        {
            TryPlaceWallTile(floorPosition + Vector2Int.up);
            TryPlaceWallTile(floorPosition + Vector2Int.down);
            TryPlaceWallTile(floorPosition + Vector2Int.left);
            TryPlaceWallTile(floorPosition + Vector2Int.right);
        }
    }

    private void TryPlaceWallTile(Vector2Int position)
    {
        // Do not place a wall on top of a floor tile.
        if (floorPositions.Contains(position))
            return;

        Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);

        // Do not place a wall if there is already one there.
        if (wallTilemap.HasTile(tilePosition))
            return;

        // Chooses the correct wall tile and places it.
        TileBase chosenWallTile = ChooseWallTile(position);
        wallTilemap.SetTile(tilePosition, chosenWallTile);
    }

    private TileBase ChooseWallTile(Vector2Int position)
    {
        // Checks if there are floor tiles around this wall position.
        bool hasFloorAbove = floorPositions.Contains(position + Vector2Int.up);
        bool hasFloorBelow = floorPositions.Contains(position + Vector2Int.down);
        bool hasFloorLeft = floorPositions.Contains(position + Vector2Int.left);
        bool hasFloorRight = floorPositions.Contains(position + Vector2Int.right);

        // If floor is above or below, use horizontal wall.
        if (hasFloorAbove || hasFloorBelow)
            return wallHorizontalTile;

        // If floor is left or right, use vertical wall.
        if (hasFloorLeft || hasFloorRight)
            return wallVerticalTile;

        // Default wall tile.
        return wallHorizontalTile;
    }

    private void PlacePlayerAndExit()
    {
        // Stops if no rooms exist.
        if (generatedRooms.Count == 0)
        {
            Debug.LogWarning("No rooms were created, so player and exit cannot be placed.");
            return;
        }

        // Player starts in the first room.
        Vector2Int playerSpawnCell = GetRoomCenter(generatedRooms[0]);

        // Exit is placed in the last room.
        Vector2Int exitCell = GetRoomCenter(generatedRooms[generatedRooms.Count - 1]);

        // Converts tile position to world position because prefabs use world positions.
        Vector3 playerWorldPosition = ConvertCellToWorldPosition(playerSpawnCell);

        // Moves player slightly forward on Z so it appears above the tilemap.
        playerWorldPosition += new Vector3(0, 0, -1);

        // Spawns the player prefab.
        spawnedPlayer = Instantiate(playerPrefab, playerWorldPosition, Quaternion.identity);

        // Places the exit tile.
        Vector3Int exitTilePosition = new Vector3Int(exitCell.x, exitCell.y, 0);
        floorTilemap.SetTile(exitTilePosition, exitTile);

        Debug.Log("Player spawned at: " + playerSpawnCell);
        Debug.Log("Exit tile placed at: " + exitCell);
    }

    private Vector3 ConvertCellToWorldPosition(Vector2Int cellPosition)
    {
        // Converts a 2D cell position into a Vector3 tilemap position.
        Vector3Int tilePosition = new Vector3Int(cellPosition.x, cellPosition.y, 0);

        // Converts tilemap cell position into world position.
        Vector3 worldPosition = floorTilemap.CellToWorld(tilePosition);

        // Moves the position to the center of the tile.
        worldPosition += new Vector3(0.5f, 0.5f, 0);

        return worldPosition;
    }

    //GetGeneratedRooms function to store the furniture and props in the FurniturePropsGenerator.cs
    public List<RectInt> GetGeneratedRooms()
    {
        return generatedRooms;
    }
}



[System.Serializable]
public class BSPNode
{
    public RectInt area;
    // The rectangle area of this partition.

    public BSPNode left;
    // First child partition after splitting.

    public BSPNode right;
    // Second child partition after splitting.

    public RectInt room;
    // The room created inside this partition.

    public BSPNode(RectInt area)
    {
        // Constructor: runs when a new BSPNode is created.
        this.area = area;
    }

    public bool IsLeaf()
    {
        // A leaf is a partition that has no children.
        return left == null && right == null;
    }
}