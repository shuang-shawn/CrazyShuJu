// 1/8/2026 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

// 1/8/2026 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    public float explosionDelay = 2f; // Time before the bomb explodes
    public float explosionRange = 3f; // Length of the explosion rays
    public GameObject explosionPrefab; // Reference to the explosion prefab
    private string ownerTag; // Tag of the player who placed the bomb
    private PlayerController ownerController; // Reference to the player controller
    private CircleCollider2D bombCollider; // Reference to the bomb's collider
    private bool exploded = false;
    public Color startColor = Color.white;
    public Color midColor = new Color(1f, 0.5f, 0f); // Orange
    public Color endColor = Color.red;
    private SpriteRenderer spriteRenderer; // Use SpriteRenderer for 2D

    private void Awake()
    {
        bombCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            Debug.Log("Re-enabling collision between bomb and player.");
            bombCollider.isTrigger = false; // Disable trigger to enable collisions
        }
    }

    public void Initialize(string ownerTag, PlayerController ownerController, float explosionRange)
    {
        this.ownerTag = ownerTag;
        this.ownerController = ownerController;
        this.explosionRange = explosionRange;
        StartCoroutine(CountdownRoutine());
    }
    IEnumerator CountdownRoutine()
    {
        float elapsed = 0f;

        while (elapsed < explosionDelay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / explosionDelay; // 0 to 1

            // Interpolate colors: White -> Orange -> Red
            if (t < 0.5f)
            {
                // First half: White to Orange
                spriteRenderer.color = Color.Lerp(startColor, midColor, t * 2f);
            }
            else
            {
                // Second half: Orange to Red
                spriteRenderer.color = Color.Lerp(midColor, endColor, (t - 0.5f) * 2f);
            }

            yield return null; // Wait until next frame
        }

        Explode();
    }

    private void Explode()
    {
        if (exploded) return; 
        exploded = true;
        Debug.Log($"{ownerTag}'s bomb exploded!");

        // Spawn explosion prefabs
        SpawnExplosions();

        // Perform hit detection using raycasts
        Vector3[] directions = {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right
        };

        foreach (var direction in directions)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, explosionRange, LayerMask.GetMask("PlayerLayer", "BlockLayer", "BombLayer"));
            Debug.DrawLine(transform.position, transform.position + direction * explosionRange, Color.red, 1f);

            // IMPORTANT: Sort by distance so we hit the closest things first
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                // 1. Check for Blocks (The "Wall" logic)
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("BlockLayer"))
                {
                    Debug.Log("Hit a block, stopping this direction.");
                    hit.collider.gameObject.GetComponent<Block>()?.HandleDestruction();
                    break;
                }

                // 2. Check for Players
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("PlayerLayer"))
                {
                    Debug.Log($"Hit {hit.collider.tag}!");
                    var player = hit.collider.gameObject.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        var renderer = player.GetComponent<SpriteRenderer>();
                        if (renderer != null)
                            renderer.color = Color.white;
                        // Stop movement capability
                        player.enabled = false;
                        // Prevent interaction: set layer to IgnoreRaycast and disable collider
                        player.gameObject.layer = LayerMask.NameToLayer("IgnoreRaycast");
                        var collider = player.GetComponent<Collider2D>();
                        if (collider != null)
                            collider.enabled = false;
                    }
                }

                // 3. Check for other Bombs
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("BombLayer") && hit.collider.gameObject != gameObject)
                {
                    Bomb otherBomb = hit.collider.GetComponent<Bomb>();
                    if (otherBomb != null) otherBomb.Explode();
                }
            }
        }

        // Notify the owner that the bomb has exploded
        ownerController.OnBombExploded();

        // Destroy the bomb after explosion
        Destroy(gameObject);
    }

    private void SpawnExplosions()
    {
        if (explosionPrefab == null)
        {
            Debug.LogError("Explosion prefab is not assigned!");
            return;
        }

        // Spawn explosions in all directions based on explosion range
        SpawnExplosionInDirection(Vector3.up, new Vector3(0, 1, 0)); // Top
        SpawnExplosionInDirection(Vector3.down, new Vector3(0, -1, 0)); // Bottom
        SpawnExplosionInDirection(Vector3.left, new Vector3(-1, 0, 0), Quaternion.Euler(0, 0, 90)); // Left
        SpawnExplosionInDirection(Vector3.right, new Vector3(1, 0, 0), Quaternion.Euler(0, 0, 90)); // Right
    }

    private void SpawnExplosionInDirection(Vector3 baseDirection, Vector3 offset, Quaternion rotation = default)
    {
        for (int i = 0; i < explosionRange; i++)
        {
            Vector3 position = transform.position + baseDirection * (i + 0.5f);
            // Check for a block at this position
            Collider2D hit = Physics2D.OverlapCircle(position, 0.2f, LayerMask.GetMask("BlockLayer"));
            if (hit != null)
            {
                // Optionally spawn explosion at the block position before breaking
                GameObject explosion = Instantiate(explosionPrefab, position, rotation);
                Destroy(explosion, 0.3f);
                break; // Stop spawning further explosions in this direction
            }
            GameObject exp = Instantiate(explosionPrefab, position, rotation);
            Destroy(exp, 0.3f); // Destroy explosion after 0.3 seconds
        }
    }
}   