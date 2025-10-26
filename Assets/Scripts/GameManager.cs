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
    public int maxSharks = 1;
    public int maxFish = 10;
    public Vector2 spawnArea = new Vector2(20f, 10f);

    private int jellyfishCount;
    private int sharkCount;
    private int fishCount;

    void Start()
    {
        SpawnAllAnimals();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        float y = Random.Range(-spawnArea.y / 2, spawnArea.y / 2);
        return new Vector3(x, y, 0f);
    }   
}
