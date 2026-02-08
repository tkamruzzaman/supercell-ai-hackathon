using System;
using UnityEngine;
using System.Collections.Generic;
using PlayerId = Enums.PlayerId;

public class HeroController : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float destructionRate = 1f;

    [Header("Followers")]
    private List<Follower> followers = new List<Follower>();
    
    private CaptureZone currentZone;
    public CaptureZone GetCurrentZone() => currentZone;

    private readonly List<IDestroyable> currentTargets = new();

    public float GetDestructionRate() => destructionRate;
    public bool HasFollowers() => followers.Count > 0;
    public PlayerId HeroControllerPlayerId { get; set; }
    
    private void OnDestroy()
    {
        PlayerIdTracker.Release(HeroControllerPlayerId);
    }

    private void Awake()
    {
        HeroControllerPlayerId = PlayerIdTracker.GetNextAvailable();
    }

    // ===== FOLLOWER MANAGEMENT =====
    
    public void AddFollower(Follower follower)
    {
        if (!followers.Contains(follower))
        {
            followers.Add(follower);
            UpdateFollowerIndices();
        }
    }
    
    public void RemoveFollower(Follower follower)
    {
        if (followers.Contains(follower))
        {
            followers.Remove(follower);
            UpdateFollowerIndices();
        }
    }
    
    public Follower GetLastFollower()
    {
        if (followers.Count > 0)
            return followers[followers.Count - 1];
        return null;
    }
    
    private void UpdateFollowerIndices()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].SetIndex(i, followers.Count);
        }
    }

    // ===== FOLLOWER GAIN =====
    public void GrantFollowers(FollowerStats stats, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(stats.GetPrefab(HeroControllerPlayerId), transform.position, Quaternion.identity);
            Follower follower = go.GetComponent<Follower>();

            // Attach to this hero
            follower.AttachToHero(transform, stats);

            // Add to hero's follower list
            AddFollower(follower);
        }

        Debug.Log($"{name} received {amount} {stats.followerName} followers!");
    }

    // ===== DEPOSIT - FIXED =====
    public bool TryDeposit(CaptureZone zone, PlayerId player)
    {
        if (!HasFollowers())
        {
            Debug.Log("[Hero] No followers to deposit");
            return false;
        }

        // Get the last follower but DON'T remove it yet (peek)
        Follower f = GetLastFollower();
        if (f == null)
            return false;

        // Ask zone if it can accept deposit
        bool success = zone.TryDepositFollower(player, f);
        
        // ONLY remove from hero if zone accepted it
        if (success)
        {
            RemoveFollower(f);
            Debug.Log($"[Hero] Follower deposited successfully. Remaining: {followers.Count}");
        }
        else
        {
            Debug.Log($"[Hero] Deposit failed - follower stays with hero");
        }
        
        return success;
    }

    // ===== ZONE TRIGGERS =====
    private void OnTriggerEnter2D(Collider2D col)
    {
        var zone = col.GetComponent<CaptureZone>();
        if (zone != null)
        {
            currentZone = zone;
            Debug.Log($"[Hero] Entered zone (State: {zone.GetCurrentState()})");
        }

        var d = col.GetComponent<IDestroyable>();
        if (d != null)
        {
            d.StartBeingAttacked(this);
            if (!currentTargets.Contains(d))
                currentTargets.Add(d);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        var zone = col.GetComponent<CaptureZone>();
        if (zone != null && currentZone == zone)
        {
            currentZone = null;
            Debug.Log($"[Hero] Exited zone");
        }

        var d = col.GetComponent<IDestroyable>();
        if (d != null)
        {
            d.StopBeingAttacked(this);
            currentTargets.Remove(d);
        }
    }
}