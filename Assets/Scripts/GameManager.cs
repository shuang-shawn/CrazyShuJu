using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameObject chestBlockPrefab;
    public GameObject movableBlockPrefab;
    public GameObject solidBlockPrefab;
    public int minDestructibleBlocks = 15;
    public int minSolidBlocks = 10;
    public int mazeWidth = 11;
    public int mazeHeight = 9;
    public float gridSize = 1f;
    [Tooltip("Chance (0..1) to spawn no power-up even when power-ups are available")]
    public float noPowerUpChance = 0.5f;

    void Start()
    {
        // SpawnMazeBlocks();
        // ...existing code...
    }

    void SpawnMazeBlocks()
    {
        System.Collections.Generic.List<Vector2> availablePositions = new System.Collections.Generic.List<Vector2>();
        int halfWidth = mazeWidth / 2;
        int halfHeight = mazeHeight / 2;
        // Generate grid points centered at (0,0)
        for (int x = -halfWidth; x <= halfWidth; x++)
        {
            for (int y = -halfHeight; y <= halfHeight; y++)
            {
                availablePositions.Add(new Vector2(x * gridSize, y * gridSize));
            }
        }

        System.Random rng = new System.Random();

        // Shuffle all available positions
        for (int i = availablePositions.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            var temp = availablePositions[i];
            availablePositions[i] = availablePositions[j];
            availablePositions[j] = temp;
        }

        // Randomly select positions for solid blocks
        System.Collections.Generic.List<Vector2> solidPositions = availablePositions.GetRange(0, minSolidBlocks);
        System.Collections.Generic.List<Vector2> destructiblePositions = availablePositions.GetRange(minSolidBlocks, minDestructibleBlocks);

        // Place solid blocks
        foreach (var pos in solidPositions)
        {
            GameObject prefab = solidBlockPrefab;
            if (prefab == null)
            {
                Debug.LogWarning("solidBlockPrefab is not assigned in GameManager.");
                continue;
            }
            GameObject blockObj = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            Block block = blockObj.GetComponent<Block>();
            if (block != null) block.Initialize(Block.BlockType.Solid);
        }

        // Place destructible/movable/chest blocks
        foreach (var pos in destructiblePositions)
        {
            // Randomly choose between chest (destructible) and movable blocks
            bool spawnChest = rng.NextDouble() < 0.5;
            if (spawnChest)
            {
                GameObject prefab = chestBlockPrefab;
                if (prefab == null)
                {
                    Debug.LogWarning("chestBlockPrefab is not assigned in GameManager.");
                    continue;
                }
                GameObject blockObj = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
                Block block = blockObj.GetComponent<Block>();
                if (block != null) block.Initialize(Block.BlockType.Destructible);
            }
            else
            {
                GameObject prefab = movableBlockPrefab;
                if (prefab == null)
                {
                    Debug.LogWarning("movableBlockPrefab is not assigned in GameManager.");
                    continue;
                }
                GameObject blockObj = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
                Block block = blockObj.GetComponent<Block>();
                if (block != null) block.Initialize(Block.BlockType.Movable);
            }
        }
    }

    public enum PowerUpType
    {
        SpeedBoost,
        ExplosionRange,
        ExtraBomb
    }

    // Returns a random available power-up type that can be spawned, or null if none available
    public PowerUpType? GetRandomAvailablePowerUpType()
    {
        System.Collections.Generic.List<PowerUpType> available = new System.Collections.Generic.List<PowerUpType>();
        if (CanSpawnSpeedBoost()) available.Add(PowerUpType.SpeedBoost);
        if (CanSpawnExplosionRange()) available.Add(PowerUpType.ExplosionRange);
        if (CanSpawnExtraBomb()) available.Add(PowerUpType.ExtraBomb);
        if (available.Count == 0) return null;

        // Chance to spawn no power-up even when some are available
        if (UnityEngine.Random.value < noPowerUpChance)
        {
            return null;
        }
        int idx = UnityEngine.Random.Range(0, available.Count);
        PowerUpType selected = available[idx];
        // Increment the counter for the selected power-up
        switch (selected)
        {
            case PowerUpType.SpeedBoost:
                IncrementSpeedBoost();
                break;
            case PowerUpType.ExplosionRange:
                IncrementExplosionRange();
                break;
            case PowerUpType.ExtraBomb:
                IncrementExtraBomb();
                break;
        }
        return selected;
    }
    public int speedBoostAmount = 5;
    public int explosionRangeAmount = 5;
    public int extraBombAmount = 5;

    // Current counters for each power-up (private, use methods to access)
    private int currentSpeedBoost = 0;
    private int currentExplosionRange = 0;
    private int currentExtraBomb = 0;

    // Singleton instance for easy access
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Methods to check if a power-up can be spawned
    public bool CanSpawnSpeedBoost() => currentSpeedBoost < speedBoostAmount;
    public bool CanSpawnExplosionRange() => currentExplosionRange < explosionRangeAmount;
    public bool CanSpawnExtraBomb() => currentExtraBomb < extraBombAmount;

    // Methods to increment current counters (call after spawning)
    public void IncrementSpeedBoost() { if (CanSpawnSpeedBoost()) currentSpeedBoost++; }
    public void IncrementExplosionRange() { if (CanSpawnExplosionRange()) currentExplosionRange++; }
    public void IncrementExtraBomb() { if (CanSpawnExtraBomb()) currentExtraBomb++; }

    // Methods to decrement (e.g., when a power-up is used or destroyed)
    public void DecrementSpeedBoost() { if (currentSpeedBoost > 0) currentSpeedBoost--; }
    public void DecrementExplosionRange() { if (currentExplosionRange > 0) currentExplosionRange--; }
    public void DecrementExtraBomb() { if (currentExtraBomb > 0) currentExtraBomb--; }

    // Methods to get current counts
    public int GetCurrentSpeedBoost() => currentSpeedBoost;
    public int GetCurrentExplosionRange() => currentExplosionRange;
    public int GetCurrentExtraBomb() => currentExtraBomb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void Update()
    {
        // ...existing code...
    }

}