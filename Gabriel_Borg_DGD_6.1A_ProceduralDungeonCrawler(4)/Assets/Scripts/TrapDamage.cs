using UnityEngine;
using UnityEngine.Tilemaps;

public class TrapDamage : MonoBehaviour
{
    [Header("Trap Tilemap")]
    [SerializeField] private Tilemap trapTilemap;

    [Header("Trap Tiles")]
    [SerializeField] private TileBase spikeTrapTile;
    [SerializeField] private TileBase poisonPoolTile;
    [SerializeField] private TileBase pressurePlateTile;

    [Header("Damage Amounts")]
    [SerializeField] private int spikeDamage = 15;
    [SerializeField] private int poisonDamage = 5;
    [SerializeField] private int pressurePlateDamage = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        Vector3Int playerCellPosition = trapTilemap.WorldToCell(collision.transform.position);

        TileBase trapTile = trapTilemap.GetTile(playerCellPosition);

        if (trapTile == spikeTrapTile)
        {
            playerHealth.TakeDamage(spikeDamage);
        }
        else if (trapTile == poisonPoolTile)
        {
            playerHealth.TakeDamage(poisonDamage);
        }
        else if (trapTile == pressurePlateTile)
        {
            playerHealth.TakeDamage(pressurePlateDamage);
        }
    }
}