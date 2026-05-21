using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    private void Attack()
    {
        // Find all enemies in the scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            // Check distance between player and enemy
            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            // If enemy is close enough
            if (distance < 2f)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damageAmount);
                }
            }
        }
    }
}