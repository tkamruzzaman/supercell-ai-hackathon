using System;
using UnityEngine;
using System.Collections.Generic;
using PlayerId = Enums.PlayerId;

public class HeroController : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float destructionRate = 1f;

    [Header("Followers")]
    [SerializeField] private HeroFollowers followerGroup;
    private CaptureZone currentZone;
    public CaptureZone GetCurrentZone() => currentZone;

    private readonly List<IDestroyable> currentTargets = new();

    public float GetDestructionRate() => destructionRate;
    public bool HasFollowers() => followerGroup.HasFollowers();
    public PlayerId HeroControllerPlayerId { get; set; }
    private void OnDestroy()
    {
        PlayerIdTracker.Release(HeroControllerPlayerId);
    }

    private void Awake()
    {
        if (followerGroup == null)
            followerGroup = GetComponent<HeroFollowers>();
        
        HeroControllerPlayerId = PlayerIdTracker.GetNextAvailable();
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

            // Add to hero's follower group
            followerGroup.AddFollower(follower);
        }

        Debug.Log($"{name} received {amount} {stats.followerName} followers!");
    }

    // ===== DEPOSIT =====
    

    public bool TryDeposit(CaptureZone zone, PlayerId player)
    {
        if (!followerGroup.HasFollowers())
            return false;

        Follower f = followerGroup.RemoveOneFollower();
        if (f == null)
            return false;

        bool success = zone.TryDepositFollower(player, f); // pass actual follower
        return success;
    }


    // ===== ATTACK =====
    private void OnTriggerEnter2D(Collider2D col)
    {
        var zone = col.GetComponent<CaptureZone>();
        if (zone != null)
        {
            currentZone = zone; // Hero entered this zone
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
            currentZone = null; // Hero left the zone
        }

        var d = col.GetComponent<IDestroyable>();
        if (d != null)
        {
            d.StopBeingAttacked(this);
            currentTargets.Remove(d);
        }
    }
}
