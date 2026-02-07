using UnityEngine;

public class Follower : MonoBehaviour
{
    private Transform hero;
    private int index;
    private int total;

    [Header("Movement")]
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float radius = 1.2f;

    private FollowerStats stats;
    public FollowerStats GetStats() => stats;

    // ======================
    // LIFECYCLE
    // ======================
    public void AttachToHero(Transform heroTransform, FollowerStats followerStats)
    {
        hero = heroTransform;
        stats = followerStats;
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
        if (hero == null || total <= 0)
            return;

        // Circular formation around hero
        float angle = index * Mathf.PI * 2f / total;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        Vector3 target = hero.position + offset;

        transform.position = Vector3.MoveTowards(transform.position, target, followSpeed * Time.deltaTime);
    }
}