using UnityEngine;

public class Shark : MonoBehaviour
{
    enum SharkState { Swimming, Chase, Feeding }

    [Header("Movement")]
    public float swimSpeed = 3f;
    public float rayCheckDistance = 5f;
    public float turnCooldown = 0.5f; // seconds to wait before next turn

    private int moveDirection = -1; // 1 = right, -1 = left
    private float turnTimer = 0f;

    void Start()
    {
        // Randomly start going left or right
        moveDirection = -1;
        FaceDirection();
    }

    void Update()
    {
        //Timer to avoid turning constantly when hitting wall
        turnTimer -= Time.deltaTime;

        // Move fish
        transform.Translate(Vector2.right * moveDirection * swimSpeed * Time.deltaTime, Space.World);

        // Raycast in front
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * moveDirection, rayCheckDistance);
        if (hit && hit.collider.CompareTag("Wall") && turnTimer <= 0f)
        {
            TurnAround();
        }
    }

    void TurnAround()
    {
        moveDirection *= -1;
        FaceDirection();

        // Push fish slightly away from the wall so it doesn’t trigger again immediately
        transform.position += Vector3.right * moveDirection * 0.2f;

        // Set cooldown
        turnTimer = turnCooldown;
    }

    void FaceDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -moveDirection;
        transform.localScale = scale;
    }
}
