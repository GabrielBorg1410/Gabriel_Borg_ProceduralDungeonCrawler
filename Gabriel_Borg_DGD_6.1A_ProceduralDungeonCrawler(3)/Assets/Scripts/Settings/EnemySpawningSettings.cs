using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawningSettings", menuName = "PCG/EnemySpawningSettings")]
public class EnemySpawningSettings : ScriptableObject
{
    [Header("Enemy Prefabs")]
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;
    public GameObject patrolEnemyPrefab;

    [Header("Enemy Spawning Settings")]
    // Minimum enemies that can spawn in one room.
    public int minEnemiesPerRoom = 1;

    // Maximum enemies that can spawn in one room.
    public int maxEnemiesPerRoom = 3;

    // Chance that a room will contain enemies.
    [Range(0f, 1f)]
    public float roomChanceToHaveEnemies = 0.6f;
}
