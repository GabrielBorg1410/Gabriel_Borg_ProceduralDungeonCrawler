using UnityEngine;

[CreateAssetMenu(fileName = "DungeonSettings", menuName = "PCG/Dungeon Settings")]
public class DungeonSettings : ScriptableObject
{
    [Header("Dungeon Size")]
    public int dungeonWidth = 100;
    public int dungeonHeight = 100;

    [Header("Room Settings")]
    public int minRoomSize = 6;
    public int maxRoomSize = 14;

    [Header("BSP Settings")]
    public int minPartitionSize = 18;
    public int maxDepth = 4;
    public int roomPadding = 2;

    [Header("Seed Settings")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Spawn Settings")]
    public bool useRandomSpawnRoom = false;
    public int minDistanceBetweenSpawnAndExit = 3;
}