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

    //These variables store the generated dungeon data. The BSP root starts as the whole dungeon area. Final partitions are the sections where rooms are created.
    private BSPNode rootPartition;
    private List<BSPNode> finalPartitions = new List<BSPNode>();
    private List<RectInt> generatedRooms = new List<RectInt>();
    private List<Vector2Int> corridorPositions = new List<Vector2Int>();
    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

    [Button("Generate Dungeon")]
    private void GenerateDungeonButton()
    {
        GenerateDungeon();
    }

    [Button("Reset Dungeon")]
    private void ResetDungeonButton()
    {
        ClearDungeon();
        Debug.Log("Dungeon reset.");
    }

    //This is the main generation order. First it creates the layout, then it paints it, then it places the player and exit.
    private void GenerateDungeon()
    {
        if (!CanGenerateDungeon())
            return;

        Debug.Log("Generating dungeon...");

        SetSeed();
        ClearDungeon();

        CreateRootPartition();
        SplitPartition(rootPartition, 0);

        CreateRoomsInsidePartitions();
        CreateCorridorsBetweenRooms();

        PaintFloorTiles();
        PaintWallTiles();

        PlacePlayerAndExit();

        Debug.Log("Rooms created: " + generatedRooms.Count);
        Debug.Log("Corridor tiles created: " + corridorPositions.Count);
    }

    //Getting the floor positions to generate different tiles that is not stone
    public HashSet<Vector2Int> GetFloorPositions()
    {
        return floorPositions;
    }

    //Checks if important references are assigned in the Inspector.This prevents null reference errors when generating.
    private bool CanGenerateDungeon()
    {
        if (settings == null)
        {
            Debug.LogError("Dungeon settings are missing.");
            return false;
        }

        if (floorTilemap == null || wallTilemap == null)
        {
            Debug.LogError("Floor or wall tilemap is missing.");
            return false;
        }

        if (floorTile == null || wallHorizontalTile == null || wallVerticalTile == null || exitTile == null)
        {
            Debug.LogError("One or more tiles are missing.");
            return false;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing.");
            return false;
        }

        return true;
    }

    //Clears the generated dungeon before making a new one. This removes old tiles, old rooms, old corridors and the previous player.
    private void ClearDungeon()
    {
        if (floorTilemap != null)
            floorTilemap.ClearAllTiles();

        if (wallTilemap != null)
            wallTilemap.ClearAllTiles();

        finalPartitions.Clear();
        generatedRooms.Clear();
        corridorPositions.Clear();
        floorPositions.Clear();

        if (spawnedPlayer != null)
            DestroyImmediate(spawnedPlayer);
    }

    //Seed based generation system. Random seed gives a different dungeon. Fixed seed repeats the same dungeon for testing.
    private void SetSeed()
    {
        if (settings.useRandomSeed)
        {
            int randomSeed = System.DateTime.Now.Millisecond;
            Random.InitState(randomSeed);
            Debug.Log("Using random seed: " + randomSeed);
        }
        else
        {
            Random.InitState(settings.seed);
            Debug.Log("Using fixed seed: " + settings.seed);
        }
    }

    //Creates the first BSP partition. This represents the whole dungeon area.
    //To create a partition RectInt is used because rooms and partitions are rectangles with a position, width and height on the dungeon grid.
    private void CreateRootPartition()
    {
        RectInt dungeonArea = new RectInt(
            0,
            0,
            settings.dungeonWidth,
            settings.dungeonHeight
        );

        rootPartition = new BSPNode(dungeonArea);
    }

    //Splitting one partition into smaller partitions. When the max depth of the partition is reached, the partition becomes final.
    private void SplitPartition(BSPNode partition, int depth)
    {
        if (partition == null)
            return;

        if (depth >= settings.maxDepth)
        {
            finalPartitions.Add(partition);
            return;
        }

        bool splitHorizontally = ShouldSplitHorizontally(partition);

        if (splitHorizontally)
            SplitPartitionHorizontally(partition, depth);
        else
            SplitPartitionVertically(partition, depth);
    }

    //Deciding which direction to split the partition. Wider areas split vertically, taller areas split horizontally.
    private bool ShouldSplitHorizontally(BSPNode partition)
    {
        if (partition.area.width > partition.area.height)
            return false;

        if (partition.area.height > partition.area.width)
            return true;

        return Random.value > 0.5f;
    }

    //Splitting a partition into bottom and top sections.
    private void SplitPartitionHorizontally(BSPNode partition, int depth)
    {
        if (partition.area.height < settings.minPartitionSize * 2)
        {
            finalPartitions.Add(partition);
            return;
        }

        int splitY = Random.Range(
            settings.minPartitionSize,
            partition.area.height - settings.minPartitionSize
        );

        partition.left = new BSPNode(
            new RectInt(
                partition.area.x,
                partition.area.y,
                partition.area.width,
                splitY
            )
        );

        partition.right = new BSPNode(
            new RectInt(
                partition.area.x,
                partition.area.y + splitY,
                partition.area.width,
                partition.area.height - splitY
            )
        );

        SplitPartition(partition.left, depth + 1);
        SplitPartition(partition.right, depth + 1);
    }

    //Splitting a partition into left and right sections.
    private void SplitPartitionVertically(BSPNode partition, int depth)
    {
        if (partition.area.width < settings.minPartitionSize * 2)
        {
            finalPartitions.Add(partition);
            return;
        }

        int splitX = Random.Range(
            settings.minPartitionSize,
            partition.area.width - settings.minPartitionSize
        );

        partition.left = new BSPNode(
            new RectInt(
                partition.area.x,
                partition.area.y,
                splitX,
                partition.area.height
            )
        );

        partition.right = new BSPNode(
            new RectInt(
                partition.area.x + splitX,
                partition.area.y,
                partition.area.width - splitX,
                partition.area.height
            )
        );

        SplitPartition(partition.left, depth + 1);
        SplitPartition(partition.right, depth + 1);
    }

    //Creating a room inside each final BSP partition. Padding keeps the room away from the partition edges.
    // Mathf.Min() is used to stop rooms becoming too large for partitions and go outside of the partitions.
    private void CreateRoomsInsidePartitions()
    {
        foreach (BSPNode partition in finalPartitions)
        {
            int maxRoomWidth = partition.area.width - settings.roomPadding * 2;
            int maxRoomHeight = partition.area.height - settings.roomPadding * 2;

            if (maxRoomWidth < settings.minRoomSize || maxRoomHeight < settings.minRoomSize)
                continue;

            int roomWidth = Random.Range(
                settings.minRoomSize,
                Mathf.Min(settings.maxRoomSize, maxRoomWidth) + 1
            );

            int roomHeight = Random.Range(
                settings.minRoomSize,
                Mathf.Min(settings.maxRoomSize, maxRoomHeight) + 1
            );

            int roomX = Random.Range(
                partition.area.x + settings.roomPadding,
                partition.area.xMax - roomWidth - settings.roomPadding + 1
            );

            int roomY = Random.Range(
                partition.area.y + settings.roomPadding,
                partition.area.yMax - roomHeight - settings.roomPadding + 1
            );

            RectInt room = new RectInt(roomX, roomY, roomWidth, roomHeight);

            partition.room = room;
            generatedRooms.Add(room);
        }
    }

    //Connecting every corridor to the next room. This makes sure all rooms are reachable when generating the dungeon.
    private void CreateCorridorsBetweenRooms()
    {
        if (generatedRooms.Count <= 1)
            return;

        for (int i = 0; i < generatedRooms.Count - 1; i++)
        {
            Vector2Int firstRoomCenter = GetRoomCenter(generatedRooms[i]);
            Vector2Int secondRoomCenter = GetRoomCenter(generatedRooms[i + 1]);

            CreateLCorridor(firstRoomCenter, secondRoomCenter);
        }
    }

    private Vector2Int GetRoomCenter(RectInt room)
    {
        int centerX = room.x + room.width / 2;
        int centerY = room.y + room.height / 2;

        return new Vector2Int(centerX, centerY);
    }

    //Creating an corridor using the L-Shaped algorithm. It randomly decides whether to go horizontal first or vertical first.
    private void CreateLCorridor(Vector2Int start, Vector2Int end)
    {
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
    // Mathf.Min() and Mathf.Max() are also used in corridors so the loop works correctly no matter which direction the corridor is created.
    private void CreateHorizontalCorridor(int startX, int endX, int y)
    {
        int minX = Mathf.Min(startX, endX);
        int maxX = Mathf.Max(startX, endX);

        for (int x = minX; x <= maxX; x++)
        {
            corridorPositions.Add(new Vector2Int(x, y));
        }
    }

    private void CreateVerticalCorridor(int startY, int endY, int x)
    {
        int minY = Mathf.Min(startY, endY);
        int maxY = Mathf.Max(startY, endY);

        for (int y = minY; y <= maxY; y++)
        {
            corridorPositions.Add(new Vector2Int(x, y));
        }
    }

    //Paints all rooms and corridors onto the floor tilemap. It also stores the floor positions for the wall tiles.
    private void PaintFloorTiles()
    {
        foreach (RectInt room in generatedRooms)
        {
            PaintRoomFloor(room);
        }

        foreach (Vector2Int corridorPosition in corridorPositions)
        {
            PaintFloorTile(corridorPosition);
        }
    }

    private void PaintRoomFloor(RectInt room)
    {
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
        floorPositions.Add(position);

        Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);
        floorTilemap.SetTile(tilePosition, floorTile);
    }

    //Painting the  wall tiles around every floor tile.
    private void PaintWallTiles()
    {
        foreach (Vector2Int floorPosition in floorPositions)
        {
            TryPlaceWallTile(floorPosition + Vector2Int.up);
            TryPlaceWallTile(floorPosition + Vector2Int.down);
            TryPlaceWallTile(floorPosition + Vector2Int.left);
            TryPlaceWallTile(floorPosition + Vector2Int.right);
        }
    }

    //Places a wall only if that position is not floor and does not already have a wall.
    private void TryPlaceWallTile(Vector2Int position)
    {
        if (floorPositions.Contains(position))
            return;

        Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);

        if (wallTilemap.HasTile(tilePosition))
            return;

        TileBase chosenWallTile = ChooseWallTile(position);
        wallTilemap.SetTile(tilePosition, chosenWallTile);
    }

    //Picks horizontal or vertical wall based on where nearby floor tiles are.
    private TileBase ChooseWallTile(Vector2Int position)
    {
        bool hasFloorAbove = floorPositions.Contains(position + Vector2Int.up);
        bool hasFloorBelow = floorPositions.Contains(position + Vector2Int.down);
        bool hasFloorLeft = floorPositions.Contains(position + Vector2Int.left);
        bool hasFloorRight = floorPositions.Contains(position + Vector2Int.right);

        if (hasFloorAbove || hasFloorBelow)
            return wallHorizontalTile;

        if (hasFloorLeft || hasFloorRight)
            return wallVerticalTile;

        return wallHorizontalTile;
    }

    //Places the player in the first room of the dungeon. Places the exit tile in the last room of the dungeon.
    private void PlacePlayerAndExit()
    {
        if (generatedRooms.Count == 0)
        {
            Debug.LogWarning("No rooms were created, so player and exit cannot be placed.");
            return;
        }

        Vector2Int playerSpawnCell = GetRoomCenter(generatedRooms[0]);
        Vector2Int exitCell = GetRoomCenter(generatedRooms[generatedRooms.Count - 1]);

        Vector3 playerWorldPosition = ConvertCellToWorldPosition(playerSpawnCell);
        playerWorldPosition += new Vector3(0, 0, -1);

        spawnedPlayer = Instantiate(playerPrefab, playerWorldPosition, Quaternion.identity);

        Vector3Int exitTilePosition = new Vector3Int(exitCell.x, exitCell.y, 0);
        floorTilemap.SetTile(exitTilePosition, exitTile);

        Debug.Log("Player spawned at: " + playerSpawnCell);
        Debug.Log("Exit tile placed at: " + exitCell);
    }

    //Converts a tilemap cell position into a world position.This is needed because prefabs use world positions, not tile positions.
    private Vector3 ConvertCellToWorldPosition(Vector2Int cellPosition)
    {
        Vector3Int tilePosition = new Vector3Int(cellPosition.x, cellPosition.y, 0);

        Vector3 worldPosition = floorTilemap.CellToWorld(tilePosition);
        worldPosition += new Vector3(0.5f, 0.5f, 0);

        return worldPosition;
    }
}

//This class represents one BSP partition. Each partition stores its area, child partitions, and the room inside it.
[System.Serializable]
public class BSPNode
{
    public RectInt area;
    public BSPNode left;
    public BSPNode right;
    public RectInt room;

    public BSPNode(RectInt area)
    {
        this.area = area;
    }

    public bool IsLeaf()
    {
        return left == null && right == null;
    }
}