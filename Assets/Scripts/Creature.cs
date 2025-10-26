using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHunger = 100f;
    public float hungerDepletionRate = 1f; // per second
    protected float currentHunger;

    [Header("Breeding")]
    public float breedCooldown = 20f;
    protected float breedTimer = 0f;

    protected bool isAlive = true;

    protected virtual void Start()
    {
        currentHunger = maxHunger;
    }

    protected virtual void Update()
    {
        if (!isAlive) return;

        HandleHunger();
        HandleBreeding();
    }

    protected virtual void HandleHunger()
    {
        currentHunger -= hungerDepletionRate * Time.deltaTime;

        if (currentHunger <= 0f)
        {
            Die();
        }
    }

    protected virtual void HandleBreeding()
    {
        breedTimer += Time.deltaTime;
        if (breedTimer >= breedCooldown)
        {
            TryBreed();
            breedTimer = 0f;
        }
    }

    protected abstract void TryBreed();

    protected virtual void Die()
    {
        isAlive = false;
        Destroy(gameObject);
    }

    public virtual void Eat(float amount)
    {
        currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
    }
}
