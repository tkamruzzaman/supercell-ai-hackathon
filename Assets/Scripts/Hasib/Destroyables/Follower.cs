using UnityEngine;

public class Follower : MonoBehaviour
{
    private Transform hero;
    private int index;
    private int total;

    [Header("Movement")]
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float radius = 0.5f;

    private FollowerStats stats;
    public FollowerStats GetStats() => stats;

    private float currentHealth;

    // ======================
    // HEALTH BAR
    // ======================
    [Header("Health Bar")]
    [SerializeField] private float maxHealthBarWidth = 1f;

    private SpriteRenderer healthBarRenderer;
    private float maxHealth;

    // ======================
    // UNITY LIFECYCLE
    // ======================
    private void Awake()
    {
        // Auto-find first SpriteRenderer in children (health bar)
        healthBarRenderer = GetComponentInChildren<SpriteRenderer>();

        if (healthBarRenderer != null)
        {
            // Ensure sliced mode works correctly
            Vector2 size = healthBarRenderer.size;
            size.x = maxHealthBarWidth;
            healthBarRenderer.size = size;
        }
    }

    // ======================
    // HEALTH
    // ======================
    public void InitializeHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Max(0f, currentHealth);
        UpdateHealthBar();
    }

    public bool IsDead() => currentHealth <= 0f;
    public float GetCurrentHealth() => currentHealth;

    private void UpdateHealthBar()
    {
        if (healthBarRenderer == null || maxHealth <= 0f) return;

        float ratio = currentHealth / maxHealth;

        Vector2 size = healthBarRenderer.size;
        size.x = maxHealthBarWidth * ratio;
        healthBarRenderer.size = size;

        healthBarRenderer.enabled = ratio > 0f;
    }

    // ======================
    // FOLLOW LOGIC
    // ======================
    public void AttachToHero(Transform heroTransform, FollowerStats followerStats)
    {
        hero = heroTransform;
        stats = followerStats;

        InitializeHealth(stats.maxHealth);
    }

    public void Detach()
    {
        hero = null;
        total = 0;
    }

    public void SetIndex(int newIndex, int totalCount)
    {
        index = newIndex;
        total = totalCount;
    }

    private void Update()
    {
        if (hero == null || total <= 0) return;

        float angle = index * Mathf.PI * 2f / total;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        Vector3 target = hero.position + offset;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            followSpeed * Time.deltaTime
        );
    }
}