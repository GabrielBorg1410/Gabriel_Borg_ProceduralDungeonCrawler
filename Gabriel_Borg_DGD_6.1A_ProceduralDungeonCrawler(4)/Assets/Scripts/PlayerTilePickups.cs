using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerTilePickups : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap pickupTilemap;
    [SerializeField] private Tilemap floorTilemap;

    [Header("Pickup Tiles")]
    [SerializeField] private TileBase unopenedChestTile;
    [SerializeField] private TileBase openedChestTile;
    [SerializeField] private TileBase healthPickupTile;
    [SerializeField] private TileBase doorKeyTile;

    [Header("Exit Tile")]
    [SerializeField] private TileBase exitTile;

    private bool hasKey = false;

    private void Update()
    {
        // Left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            TryPickupTile();
        }
    }

    private void TryPickupTile()
    {
        // Gets the player's tile position
        Vector3Int playerCellPosition =
            pickupTilemap.WorldToCell(transform.position);

        // Gets the tile under the player
        TileBase currentTile =
            pickupTilemap.GetTile(playerCellPosition);

        // Health pickup
        if (currentTile == healthPickupTile)
        {
            PlayerHealth playerHealth =
                GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.Heal(10);
            }

            pickupTilemap.SetTile(playerCellPosition, null);

            Debug.Log("Health pickup collected.");
        }

        // Door key
        else if (currentTile == doorKeyTile)
        {
            hasKey = true;

            pickupTilemap.SetTile(playerCellPosition, null);

            Debug.Log("Door key collected.");
        }

        // Chest
        else if (currentTile == unopenedChestTile)
        {
            pickupTilemap.SetTile(
                playerCellPosition,
                openedChestTile
            );

            Debug.Log("Chest opened.");
        }

        // Exit door
        Vector3Int exitCellPosition =
            floorTilemap.WorldToCell(transform.position);

        TileBase exitDoorTile =
            floorTilemap.GetTile(exitCellPosition);

        if (exitDoorTile == exitTile)
        {
            if (hasKey)
            {
                floorTilemap.SetTile(exitCellPosition, null);

                Debug.Log("Exit opened. Game Completed");
            }
            else
            {
                Debug.Log("You need a key.");
            }
        }
    }
}