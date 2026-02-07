using UnityEngine;

public class Follower : MonoBehaviour
{
    private Transform hero;                 // Hero to follow
    private int indexInGroup;               // Follower’s index in group
    private int totalFollowers;             // Total followers in group

    [SerializeField] private float followSpeed = 3f;      // Movement speed
    [SerializeField] private float radius = 1f;           // Circle radius around hero

    /// <summary>
    /// Initialize follower with hero reference and group info
    /// </summary>
    public void Initialize(Transform heroTransform, int index, int total)
    {
        hero = heroTransform;
        indexInGroup = index;
        totalFollowers = total;
    }

    private void Update()
    {
        if (hero == null || totalFollowers == 0) return;

        // Calculate angle for this follower
        float angle = 2 * Mathf.PI * indexInGroup / totalFollowers;

        // Target position on circle around hero
        Vector3 targetPos = hero.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

        // Move smoothly toward target
        transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}