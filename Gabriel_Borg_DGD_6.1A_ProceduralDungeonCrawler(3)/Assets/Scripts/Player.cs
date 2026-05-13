using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    //Multiplier used for terrain effects like water slowing the player
    private float speedMultiplier = 1f;

    // Reference to the Rigidbody2D component
    private Rigidbody2D rb;

    // Stores player input
    private Vector2 movement;

    private void Start()
    {
        // Get the Rigidbody2D attached to the player
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Get keyboard input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalize movement so diagonal movement is not faster
        movement = movement.normalized;
    }

    private void FixedUpdate()
    {
        //Move the player using physics and terrain speed multiplier
        rb.linearVelocity = movement * moveSpeed * speedMultiplier;
    }

    /*Changes the player's movement speed multiplier. Example: 1f = normal speed, 0.5f = slower movement on water */
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
}