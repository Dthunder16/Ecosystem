using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Fish2D : MonoBehaviour
{
    public float speed = 2f;
    public float hunger = 100f;
    public float hungerDepletionRate = 1f;
    public float eatRange = 0.5f;
    public float hungerGainFromFood = 40f;

    public float breedingCooldown = 10f;
    private float breedTimer = 0f;

    private string state = "Idle";
    private GameObject foodTarget;
    private Vector2 moveDir = Vector2.right;

    // Raycasting
    public float visionDistance = 3f; // how far the fish can see

    // Flee State Vars
    public float fleeSpeedMultiplier = 2f;
    public float fleeDuration = 2f;
    public float fleeEnergyCost = 10f;
    private bool isFleeing = false;
    private float fleeTimer = 0f;
    private float fleeCooldown = 0f;

    //Going back to original position after feeding
    private float verticalOffset = 0f;
    private bool movingUpAfterFeeding = false;
    public float upperBoundY = 5f;

    private SpriteRenderer sr;

    //Jellyfish Poison
    public bool isPoisoned = false;
    public float poisonDuration = 15f;
    public float poisonTimer = 0f;
    private Color originalColor;

    void Start()
    {
        //SR
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        // Random initial direction (left or right)
        if (Random.value > 0.5f)
            moveDir = Vector2.left;

        FlipSprite(moveDir);
    }

    void Update()
    {
        //Poison Management
        if(isPoisoned){
            Debug.Log("Is poisoned");
            poisonTimer += Time.deltaTime;
            hungerDepletionRate = 2.5f;
            sr.color = Color.green;
            
            if (poisonTimer >= poisonDuration)
            {
                isPoisoned = false;
                poisonTimer = 0f;
                hungerDepletionRate = 1f;
                sr.color = originalColor;
            }
        }

        //Hunger Management
        hunger -= Time.deltaTime * hungerDepletionRate;
        hunger = Mathf.Clamp(hunger, 0, 100);

        if (hunger <= 0)
        {
            Die();
            return;
        }

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

            case "Flee":
                Flee();
                break;

            case "Eat":
                EatFood();
                break;

            case "Dead":
                break;
        }

        //Breeding cooldown
        breedTimer -= Time.deltaTime;

        //Flee Cooldown
        if (fleeCooldown > 0)
            fleeCooldown -= Time.deltaTime;

        SenseEnvironment();
    }

    void Patrol()
    {
        //Horizontal movement
        Vector2 horizontalMove = moveDir * speed * Time.deltaTime;

        //Move up after eating
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

        //Constrain Fish Movement to the boundaries
        if (transform.position.x > 20f)
        {
            transform.position = new Vector2(20f, transform.position.y);
            moveDir = Vector2.left;
            FlipSprite(moveDir);
        }
        else if (transform.position.x < -20f)
        {
            transform.position = new Vector2(-20f, transform.position.y);
            moveDir = Vector2.right;
            FlipSprite(moveDir);
        }

        //Flip sprite
        if (horizontalMove.x != 0)
            FlipSprite(horizontalMove);
    }

    void FindFood()
    {
        if (foodTarget == null)
        {
            foodTarget = FindClosestFood();
            if (foodTarget == null)
            {
                Patrol(); //if there is no food go back to patrol
                return;
            }
        }

        //Move toward food
        Vector2 dir = ((Vector2)foodTarget.transform.position - (Vector2)transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);

        //Flip toward direction of movement
        FlipSprite(dir);

        //Check if close enough to eat
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

        //go back to previous position after eating
        verticalOffset = Random.Range(5f, 16f);
        movingUpAfterFeeding = true;

        state = "Idle";
    }

    void Flee()
    {
        if (!isFleeing)
        {
            isFleeing = true;
            fleeTimer = fleeDuration;
            hunger -= fleeEnergyCost;
            hunger = Mathf.Max(0, hunger);
        }

        //Move opposite direction quickly
        transform.Translate(moveDir * speed * fleeSpeedMultiplier * Time.deltaTime);

        fleeTimer -= Time.deltaTime;

        if (fleeTimer <= 0)
        {
            isFleeing = false;
            state = "Idle";
        }
    }

    void Die()
    {
        speed = 0;
        transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);
        StartCoroutine(FloatUpAndDestroy());
    }

    //Helper Functions

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

    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Jellyfish"))
        {
            isPoisoned = true;
        }
    }

    IEnumerator FloatUpAndDestroy()
    {
        float timer = 0f;
        while (timer < 5f)
        {
            transform.position += Vector3.up * Time.deltaTime * 0.5f;
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    void SenseEnvironment()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, visionDistance);

        if (hit.collider != null)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            if (hit.collider.CompareTag("Wall"))
            {
                moveDir = -moveDir;
                FlipSprite(moveDir);
            }

            if (hit.collider.CompareTag("Shark") && !isFleeing && fleeCooldown <= 0)
            {
                moveDir = -moveDir;
                FlipSprite(moveDir);

                state = "Flee";
                fleeCooldown = 3f;
            }
        }
        else
        {
            Debug.DrawRay(transform.position, moveDir * visionDistance, Color.green);
        }
    }

    void FlipSprite(Vector2 newDir)
    {
        if ((newDir.x > 0 && transform.localScale.x < 0) || (newDir.x < 0 && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
