using UnityEngine;
using System.Collections.Generic;

public class FollowerManager : MonoBehaviour
{
    public static FollowerManager Instance { get; private set; }

    [Header("Follower Prefab")]
    [SerializeField] private GameObject followerPrefab;

    // Keep track of all followers (optional: grouped per hero)
    private readonly List<GameObject> followers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Spawn followers for a hero.
    /// </summary>
    public void SpawnFollowers(FollowerStats stats, int amount, Transform hero)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject newFollower = Instantiate(followerPrefab, hero.position, Quaternion.identity);
            newFollower.GetComponent<Follower>().Initialize(hero, i, amount);
            followers.Add(newFollower);
        }
    }





    /// <summary>
    /// Remove follower from tracking (call when destroyed).
    /// </summary>
    public void RemoveFollower(GameObject follower)
    {
        if (followers.Contains(follower))
            followers.Remove(follower);

        Destroy(follower);
    }
}