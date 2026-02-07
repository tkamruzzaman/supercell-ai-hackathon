using System.Collections.Generic;
using UnityEngine;
using PlayerId = Enums.PlayerId;
using ZoneState = Enums.ZoneState;

public class CaptureZone : MonoBehaviour
{
    public event System.Action<PlayerId> OnContestResolved;
    public event System.Action<PlayerId, float> OnCapturePointsGenerated;

    [Header("Zone State")]
    [SerializeField] private ZoneState currentState = ZoneState.Neutral;
    [SerializeField] private PlayerId owner = PlayerId.None;
    [SerializeField, Range(0f, 100f)] private float controlPercent = 0f;

    private Dictionary<PlayerId, float> accumulatedPoints = new()
    {
        { PlayerId.Player1, 0f },
        { PlayerId.Player2, 0f }
    };

    private readonly List<Follower> player1Followers = new();
    private readonly List<Follower> player2Followers = new();

    [Header("Visuals")]
    [SerializeField] private float followerSpacing = 0.8f;
    [SerializeField] private float teamSeparation = 3f;

    [Header("Combat Timing")]
    [SerializeField] private float attackInterval = 2f; // Seconds between attack rounds
    [SerializeField] private bool showCombatLogs = true;

    private float combatTimer = 0f;

    private void Update()
    {
        TickCapture();
        TickCombat();
        ArrangeFollowers();
    }

    // =======================
    // Capture Logic
    // =======================
    private void TickCapture()
    {
        if (currentState != ZoneState.Capturing) return;

        PlayerId capturingPlayer = GetCapturingPlayer();
        if (capturingPlayer == PlayerId.None) return;

        float captureSpeed = GetTotalCaptureSpeed(capturingPlayer);
        if (captureSpeed <= 0f) return;

        float delta = captureSpeed * Time.deltaTime;
        controlPercent += delta;
        accumulatedPoints[capturingPlayer] += delta;
        OnCapturePointsGenerated?.Invoke(capturingPlayer, delta);

        if (controlPercent >= 100f)
        {
            controlPercent = 100f;
            LockZone(capturingPlayer);
        }
    }

    private float GetTotalCaptureSpeed(PlayerId player)
    {
        float total = 0f;
        foreach (var f in GetFollowers(player))
            total += f.GetStats().captureSpeed;
        return total;
    }

    // =======================
    // COMBAT - FIXED
    // =======================
    
    private void TickCombat()
    {
        if (currentState != ZoneState.Contested) return;
        if (player1Followers.Count == 0 || player2Followers.Count == 0) return;

        combatTimer += Time.deltaTime;

        if (combatTimer >= attackInterval)
        {
            combatTimer = 0f;
            ExecuteAttackRound();
        }
    }

    private void ExecuteAttackRound()
    {
        if (showCombatLogs)
        {
            Debug.Log("========== ATTACK ROUND START ==========");
        }

        // Team 1 attacks
        foreach (Follower attacker in player1Followers)
        {
            if (player2Followers.Count == 0) break;
            
            Follower target = ChooseTarget(player2Followers);
            PerformAttack(attacker, target, "Player1");
        }

        // Team 2 attacks
        foreach (Follower attacker in player2Followers)
        {
            if (player1Followers.Count == 0) break;
            
            Follower target = ChooseTarget(player1Followers);
            PerformAttack(attacker, target, "Player2");
        }

        if (showCombatLogs)
        {
            // Show health status
            Debug.Log("--- Team Health Status ---");
            Debug.Log($"P1 Team ({player1Followers.Count} alive):");
            foreach (var f in player1Followers)
            {
                Debug.Log($"  {f.name}: {f.GetCurrentHealth():F0}/{f.GetStats().maxHealth} HP");
            }
            Debug.Log($"P2 Team ({player2Followers.Count} alive):");
            foreach (var f in player2Followers)
            {
                Debug.Log($"  {f.name}: {f.GetCurrentHealth():F0}/{f.GetStats().maxHealth} HP");
            }
            Debug.Log("========== ATTACK ROUND END ==========\n");
        }

        // Remove dead followers
        RemoveDeadFollowers(player1Followers, "Player1");
        RemoveDeadFollowers(player2Followers, "Player2");

        // Check resolution
        CheckCombatResolution();
    }

    private void PerformAttack(Follower attacker, Follower target, string teamName)
    {
        if (attacker == null || target == null) return;

        // FIXED: Use actual damage from follower stats
        float damage = attacker.GetStats().damage;
        float healthBefore = target.GetCurrentHealth();
        
        target.TakeDamage(damage);
        
        float healthAfter = target.GetCurrentHealth();

        if (showCombatLogs)
        {
            Debug.Log($"[{teamName}] {attacker.name} ⚔️ → {target.name} " +
                     $"({damage:F0} dmg | {healthBefore:F0} → {healthAfter:F0} HP)");
        }

        // TODO: Play attack animation
        // TODO: Play hit animation
    }

    private Follower ChooseTarget(List<Follower> enemies)
    {
        if (enemies.Count == 0) return null;

        Follower weakest = enemies[0];
        float lowestHP = weakest.GetCurrentHealth();

        foreach (Follower enemy in enemies)
        {
            float hp = enemy.GetCurrentHealth();
            if (hp < lowestHP)
            {
                lowestHP = hp;
                weakest = enemy;
            }
        }

        return weakest;
    }

    private void RemoveDeadFollowers(List<Follower> team, string teamName)
    {
        for (int i = team.Count - 1; i >= 0; i--)
        {
            if (team[i].IsDead())
            {
                Follower deadFollower = team[i];
                
                if (showCombatLogs)
                {
                    Debug.Log($"💀 [{teamName}] {deadFollower.name} has been defeated!");
                }

                Destroy(deadFollower.gameObject);
                team.RemoveAt(i);
            }
        }
    }

    private void CheckCombatResolution()
    {
        bool p1Alive = player1Followers.Count > 0;
        bool p2Alive = player2Followers.Count > 0;

        if (!p1Alive && p2Alive)
        {
            ResolveContest(PlayerId.Player2);
        }
        else if (!p2Alive && p1Alive)
        {
            ResolveContest(PlayerId.Player1);
        }
    }

    // =======================
    // Deposit API - FIXED
    // =======================
    public bool TryDepositFollower(PlayerId player, Follower follower)
    {
        if (currentState == ZoneState.Locked || follower == null) return false;

        follower.Detach();
        
        // CRITICAL: Make sure health is initialized!
        if (follower.GetCurrentHealth() <= 0)
        {
            Debug.LogWarning($"Follower {follower.name} deposited with 0 health! Initializing...");
            follower.InitializeHealth(follower.GetStats().maxHealth);
        }
        
        if (player == PlayerId.Player1)
            player1Followers.Add(follower);
        else
            player2Followers.Add(follower);

        if (showCombatLogs)
        {
            Debug.Log($"[Deposit] {player} deposited {follower.name} with {follower.GetCurrentHealth():F0}/{follower.GetStats().maxHealth} HP");
        }

        UpdateZoneState();
        return true;
    }

    // =======================
    // Visual Arrangement
    // =======================
    private void ArrangeFollowers()
    {
        Vector3 p1Center = transform.position + Vector3.left * (teamSeparation / 2f);
        Vector3 p2Center = transform.position + Vector3.right * (teamSeparation / 2f);
        
        ArrangeTeamGrid(player1Followers, p1Center);
        ArrangeTeamGrid(player2Followers, p2Center);
    }

    private void ArrangeTeamGrid(List<Follower> followers, Vector3 centerPosition)
    {
        int count = followers.Count;
        if (count == 0) return;
        
        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / columns);
        
        float gridWidth = (columns - 1) * followerSpacing;
        float gridHeight = (rows - 1) * followerSpacing;
        Vector3 startPos = centerPosition - new Vector3(gridWidth / 2f, gridHeight / 2f, 0f);
        
        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int col = i % columns;
            
            Vector3 position = startPos + new Vector3(
                col * followerSpacing,
                row * followerSpacing,
                0f
            );
            
            followers[i].transform.position = position;
        }
    }

    // =======================
    // State Management
    // =======================
    private void UpdateZoneState()
    {
        bool p1Has = player1Followers.Count > 0;
        bool p2Has = player2Followers.Count > 0;

        if (p1Has && p2Has)
        {
            currentState = ZoneState.Contested;
            controlPercent = 0f;
            owner = PlayerId.None;
            combatTimer = 0f;
            
            if (showCombatLogs)
            {
                Debug.Log($"⚔️ COMBAT STARTED! P1: {player1Followers.Count} vs P2: {player2Followers.Count}");
            }
        }
        else if (p1Has || p2Has)
        {
            currentState = ZoneState.Capturing;
            owner = GetCapturingPlayer();
        }
        else
        {
            currentState = ZoneState.Neutral;
            owner = PlayerId.None;
        }
    }

    private PlayerId GetCapturingPlayer()
    {
        if (player1Followers.Count > 0 && player2Followers.Count == 0) return PlayerId.Player1;
        if (player2Followers.Count > 0 && player1Followers.Count == 0) return PlayerId.Player2;
        return PlayerId.None;
    }

    private List<Follower> GetFollowers(PlayerId player)
    {
        return player == PlayerId.Player1 ? player1Followers : player2Followers;
    }

    private void LockZone(PlayerId winner)
    {
        currentState = ZoneState.Locked;
        owner = winner;
        accumulatedPoints[winner] += 100f;
        Debug.Log($"🔒 [CaptureZone] Locked by {winner}");
    }

    public void ResolveContest(PlayerId winner)
    {
        if (winner == PlayerId.Player1) 
        {
            foreach (var f in player2Followers)
                Destroy(f.gameObject);
            player2Followers.Clear();
        }
        else if (winner == PlayerId.Player2) 
        {
            foreach (var f in player1Followers)
                Destroy(f.gameObject);
            player1Followers.Clear();
        }

        currentState = ZoneState.Capturing;
        owner = winner;
        controlPercent = 0f;
        combatTimer = 0f;
        OnContestResolved?.Invoke(winner);
        
        if (showCombatLogs)
        {
            Debug.Log($"★★★ {winner} WINS THE BATTLE! ★★★\n");
        }
    }

    // =======================
    // Public Getters
    // =======================
    public ZoneState GetCurrentState() => currentState;
    public PlayerId GetOwner() => owner;
    public float GetAccumulatedPoints(PlayerId player) => accumulatedPoints[player];
}