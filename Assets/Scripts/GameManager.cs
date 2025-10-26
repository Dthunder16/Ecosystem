using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Spawning Vars
    [Header("Animal Prefabs")]
    [SerializeField] public GameObject jellyfishPrefab;
    [SerializeField] public GameObject sharkPrefab;
    [SerializeField] public GameObject fishPrefab;

    [Header("Spawn Settings")]
    public int maxJellyfish = 3;
    public int maxSharks = 2;
    public int maxFish = 18;
    public Vector2 spawnArea = new Vector2(34f, 20f);

    private int jellyfishCount;
    private int sharkCount;
    private int fishCount;

    [Header("Seaweed Spawn Settings")]
    [SerializeField] public GameObject seaweedPrefab;
    public int maxSeaweedCount = 10;
    public float seaweedY = -7f;
    public float spawnRangeX = 17f; 
    public int maxSpawnAttempts = 10;

    void Start()
    {
        SpawnAllAnimals();
    }

    // Update is called once per frame
    void Update()
    {
        MaintainSeaweedPopulation();
    }

    void SpawnAllAnimals()
    {
        for (int i = 0; i < maxJellyfish; i++)
        {
            SpawnAnimal(jellyfishPrefab);
        }

        for (int i = 0; i < maxSharks; i++)
        {
            SpawnAnimal(sharkPrefab);
        }

        for (int i = 0; i < maxFish; i++)
        {
            SpawnAnimal(fishPrefab);
        }
    }

    void SpawnAnimal(GameObject animal){
        Vector3 pos = GetRandomSpawnPosition();
        Instantiate(animal, pos, Quaternion.identity);
    }

    Vector3 GetRandomSpawnPosition(){
        float x = Random.Range(-spawnArea.x / 2, spawnArea.x / 2);
        float y = Random.Range(-spawnArea.y / 2 + 4, spawnArea.y / 2);
        return new Vector3(x, y, 0f);
    }

    void MaintainSeaweedPopulation()
    {
        // Count current seaweed in the scene
        int currentCount = GameObject.FindGameObjectsWithTag("Seaweed").Length;

        // Spawn more until we reach the max
        while (currentCount < maxSeaweedCount)
        {
            if (TrySpawnSeaweed())
                currentCount++;
            else
                break;
        }
    }   

    void SpawnSeaweed(){
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(randomX, seaweedY, 0f);

        Instantiate(seaweedPrefab, spawnPos, Quaternion.identity);
    }

    bool TrySpawnSeaweed(){
        //Get collider radius from seaweed prefab
        CircleCollider2D prefabCollider = seaweedPrefab.GetComponent<CircleCollider2D>();

        float checkRadius = prefabCollider.radius * Mathf.Max(seaweedPrefab.transform.localScale.x, seaweedPrefab.transform.localScale.y);
        
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float randomX = Random.Range(-spawnRangeX, spawnRangeX);
            Vector3 spawnPos = new Vector3(randomX, seaweedY, 0f);

            //Check for overlap between seaweed spawns
            Collider2D hit = Physics2D.OverlapCircle(spawnPos, checkRadius);
            if (hit == null || !hit.CompareTag("Seaweed"))
            {
                Instantiate(seaweedPrefab, spawnPos, Quaternion.identity);
                return true;
            }
        }

        //If no spots can be found
        Debug.Log("No free spot found for new seaweed this frame.");
        return false;
    }
}
