using UnityEngine;
using System.Collections;

public class Shark : MonoBehaviour
{
    public float speed = 3f;
    public float visionDistance = 8f;
    public float eatRange = 0.5f;

    public float hunger = 100f;
    public float hungerDepletionRate = 1f;
    public float hungerGainFromFood = 50f;

    private Vector2 moveDir = Vector2.right;
    private GameObject targetFish = null;
    private SpriteRenderer sr;
    private Color originalColor;

    //State machine
    private enum SharkState { Idle, Hunt, Eat, Dead }
    private SharkState currentState = SharkState.Idle;

    //Poison system
    public bool isPoisoned = false;
    public float poisonDuration = 15f;
    private float poisonTimer = 0f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        //Start with a random direction
        moveDir = Random.value > 0.5f ? Vector2.right : Vector2.left;
        FlipSprite(moveDir);
    }

    void Update()
    {
        //Call Poison
        HandlePoison();

        //Handle hunger and death
        hunger -= Time.deltaTime * hungerDepletionRate;
        hunger = Mathf.Clamp(hunger, 0, 100);

        if (hunger <= 0)
        {
            ChangeState(SharkState.Dead);
        }

        //State Machine
        switch (currentState)
        {
            case SharkState.Idle:
                Patrol();
                if (hunger < 80f)
                {
                    ChangeState(SharkState.Hunt);
                }
                break;

            case SharkState.Hunt:
                HuntFish();
                break;

            case SharkState.Eat:
                EatFish();
                ChangeState(SharkState.Idle);
                break;

            case SharkState.Dead:
                Die();
                break;
        }
    }

    //State Functions

    void Patrol()
    {
        transform.Translate(moveDir * speed * Time.deltaTime);

        // Horizontal bounds check
        if (transform.position.x > 18f)
        {
            transform.position = new Vector2(18f, transform.position.y);
            moveDir = Vector2.left;
            FlipSprite(moveDir);
        }
        else if (transform.position.x < -18f)
        {
            transform.position = new Vector2(-18f, transform.position.y);
            moveDir = Vector2.right;
            FlipSprite(moveDir);
        }

        // If hungry, start scanning for food
        if (hunger < 80f)
            SenseEnvironment();
    }

    void HuntFish()
    {
        if (targetFish == null)
        {
            SenseEnvironment();
            Patrol();
            return;
        }

        //Move towards target fish
        Vector2 dir = ((Vector2)targetFish.transform.position - (Vector2)transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
        FlipSprite(dir);

        // Check if in range to eat
        if (Vector2.Distance(transform.position, targetFish.transform.position) <= eatRange)
        {
            ChangeState(SharkState.Eat);
        }
    }

    void EatFish()
    {
        if (targetFish != null)
        {
            Destroy(targetFish);
            targetFish = null;
            hunger += hungerGainFromFood;
            hunger = Mathf.Min(hunger, 100);
        }
    }

    void Die()
    {
        //Flip belly up for dead
        sr.color = Color.gray;
        speed = 0;
        transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);

        //Float up and disappear
        StartCoroutine(FloatUpAndDestroy());
    }

    //Helper Functions

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

    void ChangeState(SharkState newState)
    {
        currentState = newState;
    }

    void SenseEnvironment()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, visionDistance);

        if (hit.collider != null)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            if (hit.collider.CompareTag("Fish"))
            {
                targetFish = hit.collider.gameObject;
                ChangeState(SharkState.Hunt);
            }

            if (hit.collider.CompareTag("Wall"))
            {
                moveDir = -moveDir;
                FlipSprite(moveDir);
            }
        }
        else
        {
            Debug.DrawRay(transform.position, moveDir * visionDistance, Color.green);
            targetFish = null;
        }
    }

    void HandlePoison()
    {
        if (!isPoisoned)
            return;

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Jellyfish"))
        {
            isPoisoned = true;
        }
    }

    void FlipSprite(Vector2 dir)
    {
        if ((dir.x > 0 && transform.localScale.x < 0) ||
            (dir.x < 0 && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
