// 1/5/2026 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

// 1/5/2026 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float explosionRange = 3f; // Explosion range for bombs placed by this player
    public float moveSpeed = 5f; // Movement speed
    public float acceleration = 10f; // How quickly the player accelerates
    public float deceleration = 5f; // How quickly the player slows down
    public GameObject bombPrefab; // Reference to the bomb prefab
    public int maxBombs = 3; // Maximum bombs allowed at once
    public float gridSize = 1f; // Size of each grid cell for snapping
    public LayerMask bombLayer; // Layer for bombs to check for existing bombs

    private GameInputActions inputActions;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private string playerTag; // Store the player's tag
    private InputAction moveAction;
    private InputAction attackAction;

    private int currentBombCount = 0; // Track the number of bombs in the scene
    private Collider2D playerCollider; // Reference to the player's collider

    private void Awake()
    {
        inputActions = new GameInputActions();
        playerTag = gameObject.tag; // Get the tag assigned to the player
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        playerCollider = GetComponent<Collider2D>(); // Get the player's Collider2D component
    }

    private void OnEnable()
    {
        if (playerTag == "Player1")
        {
            inputActions.Player1.Enable();
            moveAction = inputActions.Player1.Move;
            attackAction = inputActions.Player1.Attack;
        }
        else if (playerTag == "Player2")
        {
            inputActions.Player2.Enable();
            moveAction = inputActions.Player2.Move;
            attackAction = inputActions.Player2.Attack;
        }

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        attackAction.performed += OnAttack;
    }

    private void OnDisable()
    {
        if (playerTag == "Player1")
        {
            inputActions.Player1.Disable();
        }
        else if (playerTag == "Player2")
        {
            inputActions.Player2.Disable();
        }

        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        attackAction.performed -= OnAttack;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (currentBombCount < maxBombs)
        {
            InstantiateBomb();
        }
        else
        {
            Debug.Log($"{playerTag} cannot place more bombs!");
        }
    }

    private void InstantiateBomb()
    {
        // Snap the bomb's position to the nearest grid point
        Vector3 snappedPosition = SnapToGrid(transform.position);

        // Check if there is already a bomb at the snapped position
        Collider2D existingBomb = Physics2D.OverlapPoint(snappedPosition, bombLayer);
        if (existingBomb != null)
        {
            Debug.Log($"Cannot place bomb at {snappedPosition}, a bomb already exists!");
            return; // Exit the method without creating a new bomb
        }

        // Instantiate the bomb
        GameObject bomb = Instantiate(bombPrefab, snappedPosition, Quaternion.identity);
        bomb.tag = $"{playerTag}Bomb"; // Tag the bomb to identify which player it belongs to
        currentBombCount++;

        // Force the physics system to update immediately
        Physics2D.SyncTransforms();

        // Start the explosion timer, passing the player's explosionRange
        Bomb bombScript = bomb.GetComponent<Bomb>();
        bombScript.Initialize(playerTag, this, explosionRange);
    }

    private Vector3 SnapToGrid(Vector3 originalPosition)
    {
        float snappedX = Mathf.Round(originalPosition.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(originalPosition.y / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, originalPosition.z);
    }


    private void FixedUpdate()
    {
        // Apply inertia to movement
        Vector2 targetVelocity = moveInput * moveSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        // Apply deceleration when no input is given
        if (moveInput == Vector2.zero)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }
    }

    public void OnBombExploded()
    {
        currentBombCount--; // Decrease bomb count when a bomb explodes
    }
}