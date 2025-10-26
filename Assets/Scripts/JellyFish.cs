using UnityEngine;

public class JellyFish : MonoBehaviour
{
    public float speed = 1.5f;
    public float hunger = 100f;
    public float hungerDepletionRate = 0.5f;
    public float eatRange = 0.5f;
    public float hungerGainFromFood = 30f;

    public float upperBoundY = 5f;
    public float verticalOffsetAfterEating = 2f;

    // Breeding
    public float breedingCooldown = 10f;
    private float breedTimer = 0f;
    public GameObject jellyfishPrefab; //use this for breeding

    private string state = "Idle";
    private GameObject foodTarget;
    private Vector2 moveDir = Vector2.right;

    private bool movingUpAfterFeeding = false;
    private float verticalOffset = 0f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        //Random initial direction
        if (Random.value > 0.5f)
            moveDir = Vector2.left;
    }

    void Update()
    {
        //Hunger
        hunger -= Time.deltaTime * hungerDepletionRate;
        hunger = Mathf.Clamp(hunger, 0, 100);

        //Breeding cooldown
        breedTimer -= Time.deltaTime;

        //State Machine
        switch (state)
        {
            case "Idle":
                Patrol();
                if (hunger < 50) state = "FindFood";
                break;

            case "FindFood":
                FindFood();
                break;

            case "Eat":
                EatFood();
                break;

            case "Breed":
                Breed();
                break;
        }
    }

    void Patrol()
    {
        Vector2 horizontalMove = moveDir * speed * Time.deltaTime;
        float moveY = 0f;

        if (movingUpAfterFeeding)
        {
            moveY = Mathf.Min(verticalOffset, speed * 0.5f * Time.deltaTime);

            if (transform.position.y + moveY > upperBoundY)
            {
                moveY = upperBoundY - transform.position.y;
                movingUpAfterFeeding = false;
            }

            verticalOffset -= moveY;
            if (verticalOffset <= 0f)
                movingUpAfterFeeding = false;
        }

        transform.Translate(new Vector2(horizontalMove.x, moveY), Space.World);

        //Horizontal bounds
        if (transform.position.x > 20f)
        {
            transform.position = new Vector2(20f, transform.position.y);
            moveDir = Vector2.left;
        }
        else if (transform.position.x < -20f)
        {
            transform.position = new Vector2(-20f, transform.position.y);
            moveDir = Vector2.right;
        }
    }

    void FindFood()
    {
        if (foodTarget == null)
        {
            foodTarget = FindClosestFood();
            if (foodTarget == null)
            {
                Patrol();
                return;
            }
        }

        Vector2 dir = ((Vector2)foodTarget.transform.position - (Vector2)transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, foodTarget.transform.position) <= eatRange)
        {
            state = "Eat";
        }
    }

    void EatFood()
    {
        if (foodTarget != null)
        {
            Destroy(foodTarget);
            hunger += hungerGainFromFood;
            hunger = Mathf.Min(hunger, 100);
        }

        foodTarget = null;

        //Float upward a bit after eating
        verticalOffset = verticalOffsetAfterEating;
        movingUpAfterFeeding = true;

        //Go to breeding state if cooldown is over
        if (breedTimer <= 0)
            state = "Breed";
        else
            state = "Idle";
    }

    void Breed()
    {
        if (jellyfishPrefab != null)
        {
            Vector2 spawnPos = (Vector2)transform.position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Instantiate(jellyfishPrefab, spawnPos, Quaternion.identity);

            breedTimer = breedingCooldown;
        }

        state = "Idle";
    }

    GameObject FindClosestFood()
    {
        GameObject[] allFood = GameObject.FindGameObjectsWithTag("Seaweed");
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject food in allFood)
        {
            float dist = Vector2.Distance(transform.position, food.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = food;
            }
        }

        return closest;
    }
}
