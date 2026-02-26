using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public enum BlockType
    {
        Solid,
        Destructible,
        Movable
    }

    public BlockType blockType;
    [HideInInspector]
    public GameManager gameManager; // Reference to the GameManager script
    private SpriteRenderer spriteRenderer; // Use SpriteRenderer for 2D
    public GameObject powerUpPrefab; // Reference to the PowerUp prefab

    // Push handling for movable blocks
    private Dictionary<GameObject, float> pushTimers = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, Vector2> pushDirections = new Dictionary<GameObject, Vector2>();
    private float pushRequiredTime = 0.5f; // Seconds required to push
    private int occupancyLayerMask; // Mask for players/blocks/bombs
    private bool isMoving = false;
    private float moveDuration = 0.18f; // seconds for smooth 1-unit move (increased for smoother feel)
    private Coroutine moveCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
        occupancyLayerMask = LayerMask.GetMask("PlayerLayer", "BlockLayer", "BombLayer");
    }

    public void Initialize(BlockType type)
    {
        blockType = type;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (blockType != BlockType.Movable) return;

        GameObject other = collision.gameObject;
        if (other.layer != LayerMask.NameToLayer("PlayerLayer")) return;

        Debug.Log($"MovableBlock: CollisionStay with {other.name}");

        // Determine direction the block should move (away from player)
        Vector2 dirVec = (transform.position - other.transform.position);
        Vector2 dir;
        if (Mathf.Abs(dirVec.x) > Mathf.Abs(dirVec.y))
            dir = dirVec.x > 0 ? Vector2.right : Vector2.left;
        else
            dir = dirVec.y > 0 ? Vector2.up : Vector2.down;

        // Track direction and timer per player
        if (!pushDirections.ContainsKey(other))
        {
            pushDirections[other] = dir;
            pushTimers[other] = 0f;
        }
        else if (pushDirections[other] != dir)
        {
            pushDirections[other] = dir;
            pushTimers[other] = 0f;
        }

        // Require the player to be moving toward the block to count as a push.
        // Prefer Rigidbody velocity, fall back to collision.relativeVelocity.
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
        Vector2 rbVel = otherRb != null ? otherRb.linearVelocity : Vector2.zero;
        Vector2 relVel = collision.relativeVelocity;
        Vector2 chosenVel = rbVel.magnitude >= relVel.magnitude ? rbVel : relVel;
        float dot = Vector2.Dot(chosenVel, dir);
        bool isPushing = dot > 0.1f; // threshold can be tuned

        float currentTimer = pushTimers.ContainsKey(other) ? pushTimers[other] : 0f;
        Debug.Log($"MovableBlock: {other.name} rbVel={rbVel} relVel={relVel} chosen={chosenVel} dot={dot:F2} isPushing={isPushing} timer={currentTimer:F2}");

        if (!isPushing)
        {
            if (pushTimers.ContainsKey(other) && pushTimers[other] > 0f)
                pushTimers[other] = 0f;
            return;
        }

        // Increment time using fixedDeltaTime for physics consistency
        pushTimers[other] += Time.fixedDeltaTime;

        if (pushTimers[other] >= pushRequiredTime)
        {
            TryMove(dir);
            pushTimers[other] = 0f;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (pushTimers.ContainsKey(other))
        {
            pushTimers.Remove(other);
            Debug.Log($"MovableBlock: CollisionExit - removed timer for {other.name}");
        }
        if (pushDirections.ContainsKey(other))
        {
            pushDirections.Remove(other);
            Debug.Log($"MovableBlock: CollisionExit - removed direction for {other.name}");
        }
    }

    private void TryMove(Vector2 dir)
    {
        if (isMoving) {
            Debug.Log("MovableBlock: currently moving — skipping TryMove");
            return;
        }

        Vector3 targetPos = transform.position + (Vector3)dir;
        Debug.Log($"MovableBlock: TryMove to {targetPos} dir={dir}");

        // Small occupancy check at the center of the target cell
        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.2f, occupancyLayerMask);
        if (hit != null && hit.gameObject == gameObject) hit = null;

        if (hit == null)
        {
            Debug.Log($"MovableBlock: Move OK, starting smooth move to {targetPos}");
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(SmoothMoveRoutine(transform.position, targetPos));
        }
        else
        {
            Debug.Log($"MovableBlock: Move blocked by {hit.gameObject.name} (layer {hit.gameObject.layer}) at {hit.transform.position}");
        }
    }

    private System.Collections.IEnumerator SmoothMoveRoutine(Vector3 startPos, Vector3 targetPos)
    {
        isMoving = true;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            float t = Mathf.Clamp01(elapsed / moveDuration);
            // apply smoothstep easing for nicer motion
            float eased = t * t * (3f - 2f * t); // smoothstep
            transform.position = Vector3.Lerp(startPos, targetPos, eased);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        Physics2D.SyncTransforms();
        isMoving = false;
        moveCoroutine = null;
        Debug.Log($"MovableBlock: Smooth move completed to {targetPos}");
    }

    public void HandleDestruction()
    {
        if (blockType == BlockType.Destructible || blockType == BlockType.Movable)
        {
            Debug.Log("Block destroyed!");
            // Spawn a random available power-up prefab if possible
            if (gameManager != null && powerUpPrefab != null)
            {
                var powerUpType = gameManager.GetRandomAvailablePowerUpType();
                if (powerUpType != null)
                {
                    GameObject powerUpObj = Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
                    var powerUpComponent = powerUpObj.GetComponent<PowerUp>();
                    if (powerUpComponent != null)
                    {
                        powerUpComponent.Initialize((PowerUp.PowerUpType)powerUpType);
                    }
                }
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("This block is solid and cannot be destroyed.");
        }
    }
}
