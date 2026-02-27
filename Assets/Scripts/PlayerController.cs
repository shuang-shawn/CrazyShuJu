// 1/5/2026 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

// 1/5/2026 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.SceneManagement;
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
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private string playerTag; // Store the player's tag
    private InputAction moveAction;
    private InputAction attackAction;

    private int currentBombCount = 0; // Track the number of bombs in the scene
    private Collider2D playerCollider; // Reference to the player's collider
    private bool isDead = false;

    private void Awake()
    {
        inputActions = new GameInputActions();
        playerTag = gameObject.tag; // Get the tag assigned to the player
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        // Play bomb-placement animation if available
        if (animator != null)
        {
            animator.SetTrigger("SetBomb");
        }

        // Force the physics system to update immediately
        Physics2D.SyncTransforms();

        // Start the explosion timer, passing the player's explosionRange
        Bomb bombScript = bomb.GetComponent<Bomb>();
        bombScript.Initialize(playerTag, this, explosionRange);

        // Play bomb placement sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.bombPlaceSFX);
        }
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

        // Animator Speed parameter: use actual velocity when moving, but if player is holding input
        // keep a non-zero Speed so walking animation continues even if movement is blocked.
        // if (animator != null)
        // {
        //     float animSpeed = rb.linearVelocity.magnitude;
        //     if (moveInput != Vector2.zero && animSpeed < 0.01f)
        //     {
        //         // Player is attempting to move but is blocked — give small non-zero speed to keep walking animation.
        //         animSpeed = moveSpeed;
        //     }
        //     animator.SetFloat("Speed", animSpeed);
        // }

        // // Flip sprite horizontally when input is left/right. Use input so holding left keeps sprite flipped
        // if (spriteRenderer != null)
        // {
        //     if (moveInput.x < 0f) spriteRenderer.flipX = true;
        //     else if (moveInput.x > 0f) spriteRenderer.flipX = false;
        //     // if moveInput.x == 0, leave current facing direction unchanged
        // }

        if (animator != null)
        {
            if (moveInput.sqrMagnitude > 0.01f) {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
            }
            if (rb.linearVelocity.magnitude > 8.9f)
            {
                animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            } else {
                animator.SetFloat("Speed", moveInput.magnitude);
            }
        }
    }

    public void OnBombExploded()
    {
        currentBombCount--; // Decrease bomb count when a bomb explodes
    }

    // Called when this player is hit by an explosion
    public void HandleExplosionHit()
    {
        if (isDead) return;
        isDead = true;

        // Play player hit sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerHitSFX);
        }

        var renderer = GetComponent<SpriteRenderer>();
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }

        // Stop movement capability
        enabled = false;

        // Stop any ongoing velocity to prevent drifting
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Prevent interaction: set layer to IgnoreRaycast and disable collider
        gameObject.layer = LayerMask.NameToLayer("IgnoreRaycast");
        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        // After 3 seconds, load appropriate win scene
        StartCoroutine(DeathAndLoadRoutine());
    }

    private System.Collections.IEnumerator DeathAndLoadRoutine()
    {
        yield return new WaitForSeconds(3f);

        // If this object is Player1, load JuWinScene (player1 was hit -> Ju loses?),
        // if Player2 was hit, load ShuangWinScene.
        if (playerTag == "Player1")
        {
            SceneManager.LoadScene("JuWinScene");
        }
        else if (playerTag == "Player2")
        {
            SceneManager.LoadScene("ShuangWinScene");
        }
    }
}
