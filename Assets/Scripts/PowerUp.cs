using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Sprites for each power-up type")]
    public Sprite speedBoostSprite;
    public Sprite extraBombSprite;
    public Sprite explosionRangeSprite;

    public enum PowerUpType
    {
        SpeedBoost,
        ExtraBomb,
        ExplosionRange
    }

    public PowerUpType powerUpType;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.pickupSFX);
            }

            Destroy(gameObject);
        }
    }

    public void Initialize(PowerUpType type)
    {
        powerUpType = type;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        switch (powerUpType)
        {
            case PowerUpType.SpeedBoost:
                if (speedBoostSprite != null) spriteRenderer.sprite = speedBoostSprite;
                Debug.Log("This is a Speed Boost power-up");
                break;
            case PowerUpType.ExtraBomb:
                if (extraBombSprite != null) spriteRenderer.sprite = extraBombSprite;
                Debug.Log("This is an Extra Bomb power-up");
                break;
            case PowerUpType.ExplosionRange:
                if (explosionRangeSprite != null) spriteRenderer.sprite = explosionRangeSprite;
                Debug.Log("This is an Explosion Range power-up");
                break;
        }
    }
}
