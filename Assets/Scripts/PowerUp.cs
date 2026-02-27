using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.SpeedBoost:
                    player.moveSpeed += 1f;
                    Debug.Log("Player received Speed Boost: +1 moveSpeed");
                    break;
                case PowerUpType.ExtraBomb:
                    player.maxBombs += 1;
                    Debug.Log("Player received Extra Bomb: +1 maxBombs");
                    break;
                case PowerUpType.ExplosionRange:
                    player.explosionRange += 1f;
                    Debug.Log("Player received Explosion Range: +1 explosionRange");
                    break;
            }

            // Play pickup sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.pickupSFX);
            }

            Destroy(gameObject);
        }
    }
    private SpriteRenderer spriteRenderer;

    public enum PowerUpType
    {
        SpeedBoost,
        ExtraBomb,
        ExplosionRange
    }
    public PowerUpType powerUpType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Initialize(PowerUpType type)
    {
        powerUpType = type;
        if (powerUpType == PowerUpType.SpeedBoost)
        {
            Debug.Log("This is a Speed Boost power-up");
            spriteRenderer.color = new Color(0f, 0f, 1f);
        }
        else if (powerUpType == PowerUpType.ExtraBomb)
        {
            Debug.Log("This is an Extra Bomb power-up");
            spriteRenderer.color = new Color(0.0f, 1.0f, 0.0f); // Example color for Extra Bomb
        }
        else if (powerUpType == PowerUpType.ExplosionRange)
        {
            Debug.Log("This is an Explosion Range power-up");
            spriteRenderer.color = new Color(1.0f, 0.0f, 0.0f); // Example color for Explosion Range
        }
    }
}
