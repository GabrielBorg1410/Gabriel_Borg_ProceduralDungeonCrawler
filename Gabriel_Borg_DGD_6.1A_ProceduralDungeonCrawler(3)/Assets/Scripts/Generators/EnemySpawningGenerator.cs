using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnemySpawningGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private EnemySpawningSettings settings;

    [Header("Dungeon Generator")]
    // Used to get the generated rooms from the dungeon.
    [SerializeField] private DungeonGenerator dungeonGenerator;

    [Header("Enemy Parent")]
    // Empty object used to keep spawned enemies organised in the hierarchy.
    [SerializeField] private Transform enemyParent;

    [Button("Generate Enemies")]
    private void GenerateEnemiesButton()
    {
        GenerateEnemies(dungeonGenerator.GetGeneratedRooms());
    }

    [Button("Clear Enemies")]
    public void ClearEnemies()
    {
        if (enemyParent == null)
            return;

        for (int i = enemyParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(enemyParent.GetChild(i).gameObject);
        }
    }

    //Spawns enemies inside the generated dungeon rooms.
    public void GenerateEnemies(List<RectInt> rooms)
    {
        // Removes old enemies first.
        ClearEnemies();

        // Stops if no rooms exist.
        if (rooms.Count == 0)
            return;

        // Loops through every generated room.
        foreach (RectInt room in rooms)
        {
            // Random chance for this room to contain enemies.
            if (Random.value > settings.roomChanceToHaveEnemies)
                continue;

            // Random amount of enemies in this room.
            int enemyCount = Random.Range(settings.minEnemiesPerRoom, settings.maxEnemiesPerRoom + 1);

            for (int i = 0; i < enemyCount; i++)
            {
                // Gets a random position inside the room.
                Vector3 spawnPosition = GetRandomWorldPosition(room);

                // Gets a random enemy prefab.
                GameObject selectedEnemy = GetRandomEnemyPrefab();

                // Spawns the enemy prefab.
                GameObject spawnedEnemy = Instantiate(selectedEnemy, spawnPosition, Quaternion.identity);

                // Puts the enemy under the enemy parent object.
                if (enemyParent != null)
                    spawnedEnemy.transform.parent = enemyParent;
            }
        }
    }

    //Gets a random world position inside a room. +1 and -1 keep enemies away from the room walls.
    private Vector3 GetRandomWorldPosition(RectInt room)
    {
        int x = Random.Range(room.xMin + 1, room.xMax - 1);
        int y = Random.Range(room.yMin + 1, room.yMax - 1);

        return new Vector3(x + 0.5f, y + 0.5f, -1f);
    }

    //Randomly chooses one enemy prefab.
    private GameObject GetRandomEnemyPrefab()
    {
        int randomNumber = Random.Range(0, 3);

        if (randomNumber == 0)
            return settings.meleeEnemyPrefab;

        if (randomNumber == 1)
            return settings.rangedEnemyPrefab;

        return settings.patrolEnemyPrefab;
    }
}