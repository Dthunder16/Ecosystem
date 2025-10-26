using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Fish : MonoBehaviour
{
    // States
    enum FishState { Swimming, Fleeing, Feeding }
    FishState currentState = FishState.Swimming;

    public float swimSpeed = 1f;
    public float fleeSpeed = 6f;
    public float fleeDuration = 0.5f;
    public float feedTime = 3f;

    float feedTimer = 0f;
    float fleeTimer = 0f;

    [Header("Wall Detection")]
    [SerializeField] private float rayCheckDistance = 1.0f;
    [SerializeField] private float turnCooldown = 0.5f;
    private float turnTimer = 0f;

    private int moveDirection = 1; // 1 = right, -1 = left
    private CircleCollider2D fishCollider;

    void Start()
    {
        moveDirection = Random.value > 0.5f ? 1 : -1;
        fishCollider = GetComponent<CircleCollider2D>();
        FaceDirection();
    }

    void Update()
    {
        turnTimer -= Time.deltaTime;

        switch (currentState)
        {
            case FishState.Swimming:
                Swim();
                break;

            case FishState.Fleeing:
                Flee();
                break;

            case FishState.Feeding:
                Feed();
                break;
        }
    }

    void Swim()
    {
        // Move
        transform.Translate(Vector2.right * moveDirection * swimSpeed * Time.deltaTime, Space.World);

        // Check for walls
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * moveDirection, rayCheckDistance);
        if (hit && hit.collider.CompareTag("Wall") && turnTimer <= 0f)
        {
            TurnAround();
        }

        // Randomly decide to feed sometimes
        if (Random.value < 0.0005f)
            SwitchState(FishState.Feeding);
    }

    void Flee()
    {
        transform.Translate(Vector2.right * moveDirection * fleeSpeed * Time.deltaTime, Space.World);
        fleeTimer += Time.deltaTime;

        if (fleeTimer >= fleeDuration)
            SwitchState(FishState.Swimming);
    }

    void Feed()
    {
        feedTimer += Time.deltaTime;
        transform.position += Vector3.up * Mathf.Sin(Time.time * 2f) * 0.001f;

        if (feedTimer >= feedTime)
            SwitchState(FishState.Swimming);
    }

    void TurnAround()
    {
        moveDirection *= -1;
        FaceDirection();

        // Push away slightly to avoid retrigger
        transform.position += Vector3.right * moveDirection * 0.2f;

        // Cooldown
        turnTimer = turnCooldown;
    }

    void FaceDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * moveDirection;
        transform.localScale = scale;
    }

    void SwitchState(FishState newState)
    {
        currentState = newState;
        feedTimer = 0f;
        fleeTimer = 0f;
    }
}
