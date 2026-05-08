using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private DungeonSettings settings;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallHorizontalTile;
    [SerializeField] private TileBase wallVerticalTile;
    [SerializeField] private TileBase exitTile;

    /*
    The player is a GameObject because it needs movement and gameplay logic.
    The exit is now a tile from the tileset.
    */
    [Header("Spawn Object")]
    [SerializeField] private GameObject playerPrefab;

    private GameObject spawnedPlayer;

    private BSPNode rootNode;

    private readonly List<BSPNode> leafNodes = new List<BSPNode>();
    private readonly List<RectInt> rooms = new List<RectInt>();
    private readonly List<Vector2Int> corridors = new List<Vector2Int>();
    private readonly HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

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

    private void GenerateDungeon()
    {
        if (!CanGenerateDungeon())
            return;

        Debug.Log("Generating dungeon...");

        SetSeed();
        ClearDungeon();

        CreateRootNode();
        SplitNode(rootNode, 0);

        CreateRooms();
        CreateCorridors();

        PaintFloorTiles();
        PaintWallTiles();

        SpawnPlayerAndExitTile();

        Debug.Log("Rooms created: " + rooms.Count);
        Debug.Log("Corridor tiles created: " + corridors.Count);
    }

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

    private void ClearDungeon()
    {
        if (floorTilemap != null)
            floorTilemap.ClearAllTiles();

        if (wallTilemap != null)
            wallTilemap.ClearAllTiles();

        leafNodes.Clear();
        rooms.Clear();
        corridors.Clear();
        floorPositions.Clear();

        if (spawnedPlayer != null)
            DestroyImmediate(spawnedPlayer);
    }

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

    private void CreateRootNode()
    {
        RectInt dungeonArea = new RectInt(
            0,
            0,
            settings.dungeonWidth,
            settings.dungeonHeight
        );

        rootNode = new BSPNode(dungeonArea);
    }

    private void SplitNode(BSPNode node, int depth)
    {
        if (node == null)
            return;

        if (depth >= settings.maxDepth)
        {
            leafNodes.Add(node);
            return;
        }

        bool splitHorizontally = ShouldSplitHorizontally(node);

        if (splitHorizontally)
        {
            SplitHorizontally(node, depth);
        }
        else
        {
            SplitVertically(node, depth);
        }
    }

    private bool ShouldSplitHorizontally(BSPNode node)
    {
        if (node.area.width > node.area.height)
            return false;

        if (node.area.height > node.area.width)
            return true;

        return Random.value > 0.5f;
    }

    private void SplitHorizontally(BSPNode node, int depth)
    {
        if (node.area.height < settings.minPartitionSize * 2)
        {
            leafNodes.Add(node);
            return;
        }

        int splitY = Random.Range(
            settings.minPartitionSize,
            node.area.height - settings.minPartitionSize
        );

        node.left = new BSPNode(
            new RectInt(
                node.area.x,
                node.area.y,
                node.area.width,
                splitY
            )
        );

        node.right = new BSPNode(
            new RectInt(
                node.area.x,
                node.area.y + splitY,
                node.area.width,
                node.area.height - splitY
            )
        );

        SplitNode(node.left, depth + 1);
        SplitNode(node.right, depth + 1);
    }

    private void SplitVertically(BSPNode node, int depth)
    {
        if (node.area.width < settings.minPartitionSize * 2)
        {
            leafNodes.Add(node);
            return;
        }

        int splitX = Random.Range(
            settings.minPartitionSize,
            node.area.width - settings.minPartitionSize
        );

        node.left = new BSPNode(
            new RectInt(
                node.area.x,
                node.area.y,
                splitX,
                node.area.height
            )
        );

        node.right = new BSPNode(
            new RectInt(
                node.area.x + splitX,
                node.area.y,
                node.area.width - splitX,
                node.area.height
            )
        );

        SplitNode(node.left, depth + 1);
        SplitNode(node.right, depth + 1);
    }

    private void CreateRooms()
    {
        foreach (BSPNode leaf in leafNodes)
        {
            int maxRoomWidth = leaf.area.width - settings.roomPadding * 2;
            int maxRoomHeight = leaf.area.height - settings.roomPadding * 2;

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
                leaf.area.x + settings.roomPadding,
                leaf.area.xMax - roomWidth - settings.roomPadding + 1
            );

            int roomY = Random.Range(
                leaf.area.y + settings.roomPadding,
                leaf.area.yMax - roomHeight - settings.roomPadding + 1
            );

            RectInt room = new RectInt(roomX, roomY, roomWidth, roomHeight);

            leaf.room = room;
            rooms.Add(room);
        }
    }

    private void CreateCorridors()
    {
        if (rooms.Count <= 1)
            return;

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int firstRoomCenter = GetRoomCenter(rooms[i]);
            Vector2Int secondRoomCenter = GetRoomCenter(rooms[i + 1]);

            CreateLCorridor(firstRoomCenter, secondRoomCenter);
        }
    }

    private Vector2Int GetRoomCenter(RectInt room)
    {
        int centerX = room.x + room.width / 2;
        int centerY = room.y + room.height / 2;

        return new Vector2Int(centerX, centerY);
    }

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

    private void CreateHorizontalCorridor(int startX, int endX, int y)
    {
        int minX = Mathf.Min(startX, endX);
        int maxX = Mathf.Max(startX, endX);

        for (int x = minX; x <= maxX; x++)
        {
            corridors.Add(new Vector2Int(x, y));
        }
    }

    private void CreateVerticalCorridor(int startY, int endY, int x)
    {
        int minY = Mathf.Min(startY, endY);
        int maxY = Mathf.Max(startY, endY);

        for (int y = minY; y <= maxY; y++)
        {
            corridors.Add(new Vector2Int(x, y));
        }
    }

    private void PaintFloorTiles()
    {
        foreach (RectInt room in rooms)
        {
            PaintRoom(room);
        }

        foreach (Vector2Int corridorPosition in corridors)
        {
            PaintFloor(corridorPosition);
        }
    }

    private void PaintRoom(RectInt room)
    {
        for (int x = room.x; x < room.xMax; x++)
        {
            for (int y = room.y; y < room.yMax; y++)
            {
                PaintFloor(new Vector2Int(x, y));
            }
        }
    }

    private void PaintFloor(Vector2Int position)
    {
        floorPositions.Add(position);

        Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);
        floorTilemap.SetTile(tilePosition, floorTile);
    }

    private void PaintWallTiles()
    {
        foreach (Vector2Int floorPosition in floorPositions)
        {
            TryPlaceWall(floorPosition + Vector2Int.up);
            TryPlaceWall(floorPosition + Vector2Int.down);
            TryPlaceWall(floorPosition + Vector2Int.left);
            TryPlaceWall(floorPosition + Vector2Int.right);
        }
    }

    private void TryPlaceWall(Vector2Int position)
    {
        if (floorPositions.Contains(position))
            return;

        Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);

        if (wallTilemap.HasTile(tilePosition))
            return;

        TileBase wallTile = ChooseWallTile(position);
        wallTilemap.SetTile(tilePosition, wallTile);
    }

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

    /*
    Player is spawned as a prefab.
    Exit is placed as a tile in the last room.
    */
    private void SpawnPlayerAndExitTile()
    {
        if (rooms.Count == 0)
        {
            Debug.LogWarning("No rooms were created, so player and exit cannot be placed.");
            return;
        }

        Vector2Int playerSpawnCell = GetRoomCenter(rooms[0]);
        Vector2Int exitCell = GetRoomCenter(rooms[rooms.Count - 1]);

        Vector3 playerWorldPosition = GetWorldPositionFromCell(playerSpawnCell) + new Vector3( 0, 0, -1);

        spawnedPlayer = Instantiate(playerPrefab, playerWorldPosition, Quaternion.identity);

        Vector3Int exitTilePosition = new Vector3Int(exitCell.x, exitCell.y, 0);

        /*
        The exit tile is placed on the floor tilemap.
        */
        floorTilemap.SetTile(exitTilePosition, exitTile);

        Debug.Log("Player spawned at: " + playerSpawnCell);
        Debug.Log("Exit tile placed at: " + exitCell);
    }

    private Vector3 GetWorldPositionFromCell(Vector2Int cellPosition)
    {
        Vector3Int tilePosition = new Vector3Int(cellPosition.x, cellPosition.y, 0);

        Vector3 worldPosition = floorTilemap.CellToWorld(tilePosition);
        worldPosition += new Vector3(0.5f, 0.5f, 0);

        return worldPosition;
    }
}

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