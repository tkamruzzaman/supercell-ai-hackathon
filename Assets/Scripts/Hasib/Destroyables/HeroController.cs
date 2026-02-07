using System;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float destructionRate = 1f; // damage per second

    // Track which destroyables are currently being attacked
    private readonly System.Collections.Generic.List<IDestroyable> currentTargets = new();

    /// <summary>
    /// Called by destroyables to get the hero's damage rate
    /// </summary>
    public float GetDestructionRate() => destructionRate;

    /// <summary>
    /// Called by destroyable to give followers
    /// </summary>
    private void Awake()
    {
        GameManager.count++;
        gameObject.name = "Player_" + GameManager.count;
        
    }

    public void GrantFollowers(FollowerStats stats, int amount)
    {
        if (FollowerManager.Instance != null)
        {
            FollowerManager.Instance.SpawnFollowers(stats, amount, transform);
            Debug.Log($"{name} received {amount} {stats.name} followers!");
        }
        else
        {
            Debug.LogWarning("No FollowerManager instance in scene!");
        }
    }

    // ======================
    // Destroyable Detection
    // ======================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var destroyable = collision.GetComponent<IDestroyable>();
        if (destroyable != null)
        {
            destroyable.StartBeingAttacked(this);
            if (!currentTargets.Contains(destroyable))
                currentTargets.Add(destroyable);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var destroyable = collision.GetComponent<IDestroyable>();
        if (destroyable != null)
        {
            destroyable.StopBeingAttacked(this);
            currentTargets.Remove(destroyable);
        }
    }
}