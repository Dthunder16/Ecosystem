using UnityEngine;
using System.Collections.Generic;

public class Shark : MonoBehaviour
{
    public float speed = 3f;
    public float visionDistance = 5f; // how far the shark can see
    public float eatRange = 0.5f;

    public float hunger = 100f;
    public float hungerDepletionRate = 1f; // per second
    public float hungerGainFromFood = 50f;

    private Vector2 moveDir = Vector2.right;
    private GameObject targetFish = null;
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

        //Random direction
        moveDir = Random.value > 0.5f ? Vector2.right : Vector2.left;
        FlipSprite(moveDir);
    }

    void Update()
    {
        //Poison Management
        if(isPoisoned){
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

        // Sense environment for fish only if hungry
        if (hunger < 80f)
            SenseEnvironment();

        // Move
        PatrolOrChase();
    }

    void PatrolOrChase()
    {
        Vector2 moveVector = moveDir * speed * Time.deltaTime;

        if (targetFish != null)
        {
            // Move towards target fish
            Vector2 dir = ((Vector2)targetFish.transform.position - (Vector2)transform.position).normalized;
            moveVector = dir * speed * Time.deltaTime;

            // Flip sprite toward fish
            FlipSprite(dir);

            // Check if in eat range
            if (Vector2.Distance(transform.position, targetFish.transform.position) <= eatRange)
            {
                EatFish();
            }
        }

        // Apply movement
        transform.Translate(moveVector);

        // Horizontal screen bounds check
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
            targetFish = null; // no fish in sight
        }
    }

    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Jellyfish"))
        {
            isPoisoned = true;
        }
    }

    void EatFish()
    {
        if (targetFish != null)
        {
            Destroy(targetFish);
            hunger += hungerGainFromFood;
            hunger = Mathf.Min(hunger, 100);
            targetFish = null;
        }
    }

    void FlipSprite(Vector2 dir)
    {
        if ((dir.x > 0 && transform.localScale.x < 0) || (dir.x < 0 && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    void Die()
    {
        // Simple destroy for now; could add float-up or other animation
        Destroy(gameObject);
    }
}
