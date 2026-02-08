using System.Collections.Generic;
using UnityEngine;
using PlayerId = Enums.PlayerId;
using ZoneState = Enums.ZoneState;

public class CaptureZone : MonoBehaviour
{
    public event System.Action<PlayerId> OnContestResolved;
    public event System.Action<PlayerId, float> OnCapturePointsGenerated;
    
    [Header("Capture Visuals")]
    [SerializeField] private Transform captureBarFill;
    [SerializeField] private float maxCaptureBarWidth = 2f;
    [SerializeField] private Color player1Color = Color.blue;
    [SerializeField] private Color player2Color = Color.red;

    [Header("Zone State")]
    [SerializeField] private ZoneState currentState = ZoneState.Neutral;
    [SerializeField] private PlayerId owner = PlayerId.None;
    [SerializeField, Range(0f, 100f)] private float controlPercent = 0f;
    
    [Header("Combat Visuals")]
    [SerializeField] private GameObject combatAnimation;

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
    
    [Header("Combat Balance")]
    [SerializeField, Range(0.01f, 1f)]
    private float damagePercentPerHit = 0.1f;

    [Header("Combat Timing")]
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private bool showCombatLogs = true;

    private float combatTimer = 0f;

    private void Update()
    {
        TickCapture();
        TickCombat();
        ArrangeFollowers();
        UpdateCaptureBar();
    }

    private void UpdateCaptureBar()
    {
        if (captureBarFill == null) return;

        float ratio = Mathf.Clamp01(controlPercent / 100f);
        captureBarFill.localScale = new Vector3(maxCaptureBarWidth * ratio, captureBarFill.localScale.y, 1f);

        PlayerId capturingPlayer = GetCapturingPlayer();
        var sr = captureBarFill.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = capturingPlayer == PlayerId.Player1 ? player1Color :
                capturingPlayer == PlayerId.Player2 ? player2Color : Color.gray;
        }

        captureBarFill.gameObject.SetActive(capturingPlayer != PlayerId.None);
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
    // COMBAT
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
            Debug.Log("========== ATTACK ROUND START ==========");

        // Team 1 attacks
        foreach (Follower attacker in player1Followers.ToArray())
        {
            if (player2Followers.Count == 0) break;

            Follower target = ChooseTarget(player2Followers);
            PerformAttack(attacker, target, "Player1");

            if (target.IsDead())
                RemoveFollower(target, player2Followers, "Player2");
        }

        // Team 2 attacks
        foreach (Follower attacker in player2Followers.ToArray())
        {
            if (player1Followers.Count == 0) break;

            Follower target = ChooseTarget(player1Followers);
            PerformAttack(attacker, target, "Player2");

            if (target.IsDead())
                RemoveFollower(target, player1Followers, "Player1");
        }

        if (showCombatLogs)
            PrintTeamStatus();

        CheckCombatResolution();
    }

    private void PerformAttack(Follower attacker, Follower target, string teamName)
    {
        if (attacker == null || target == null) return;

        float maxHP = target.GetStats().maxHealth;
        float damage = maxHP * damagePercentPerHit;

        float healthBefore = target.GetCurrentHealth();
        target.TakeDamage(damage);
        float healthAfter = target.GetCurrentHealth();

        if (showCombatLogs)
        {
            Debug.Log(
                $"[{teamName}] {attacker.name} ⚔️ → {target.name} " +
                $"({damage:F1} dmg | {healthBefore:F1} → {healthAfter:F1} HP)"
            );
        }
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

    private void RemoveFollower(Follower follower, List<Follower> team, string teamName)
    {
        if (follower == null) return;

        if (showCombatLogs)
            Debug.Log($"💀 [{teamName}] {follower.name} has been defeated!");

        team.Remove(follower);
        Destroy(follower.gameObject);
    }

    private void PrintTeamStatus()
    {
        Debug.Log("--- Team Health Status ---");
        Debug.Log($"P1 Team ({player1Followers.Count} alive):");
        foreach (var f in player1Followers)
            Debug.Log($"  {f.name}: {f.GetCurrentHealth():F0}/{f.GetStats().maxHealth} HP");

        Debug.Log($"P2 Team ({player2Followers.Count} alive):");
        foreach (var f in player2Followers)
            Debug.Log($"  {f.name}: {f.GetCurrentHealth():F0}/{f.GetStats().maxHealth} HP");

        Debug.Log("========== ATTACK ROUND END ==========\n");
    }

    private void CheckCombatResolution()
    {
        bool p1Alive = player1Followers.Count > 0;
        bool p2Alive = player2Followers.Count > 0;

        if (!p1Alive && p2Alive)
            ResolveContest(PlayerId.Player2);
        else if (!p2Alive && p1Alive)
            ResolveContest(PlayerId.Player1);
        else if (!p1Alive && !p2Alive)
        {
            currentState = ZoneState.Neutral;
            owner = PlayerId.None;
            controlPercent = 0f;
            if (showCombatLogs)
                Debug.Log("💀 Both teams wiped out! Zone resets to Neutral.");

            OnContestResolved?.Invoke(PlayerId.None);

            if (combatAnimation != null)
                combatAnimation.SetActive(false);
        }
    }

    // =======================
    // Deposit API
    // =======================
    public bool TryDepositFollower(PlayerId player, Follower follower)
    {
        if (currentState == ZoneState.Locked)
        {
            if (showCombatLogs)
                Debug.Log($"[Deposit REJECTED] Zone is LOCKED by {owner}");
            return false;
        }

        if (follower == null)
        {
            Debug.LogError("[Deposit REJECTED] Follower is null");
            return false;
        }

        if (follower.GetCurrentHealth() <= 0)
        {
            Debug.LogWarning($"[Deposit] {follower.name} has 0 HP, initializing...");
            follower.InitializeHealth(follower.GetStats().maxHealth);
        }

        follower.Detach();

        if (player == PlayerId.Player1)
            player1Followers.Add(follower);
        else
            player2Followers.Add(follower);

        if (showCombatLogs)
            Debug.Log($"[Deposit SUCCESS] {player} deposited {follower.name} with {follower.GetCurrentHealth():F0}/{follower.GetStats().maxHealth} HP");

        UpdateZoneState();
        return true;
    }

    // =======================
    // Visual Arrangement
    // =======================
    private void ArrangeFollowers()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Vector2 zoneCenter = box.bounds.center;
        Vector2 zoneSize = box.bounds.size;

        Vector3 p1Center = zoneCenter + Vector2.left * (zoneSize.x / 4f);
        Vector3 p2Center = zoneCenter + Vector2.right * (zoneSize.x / 4f);

        ArrangeTeamGrid(player1Followers, p1Center, zoneSize / 2f);
        ArrangeTeamGrid(player2Followers, p2Center, zoneSize / 2f);
    }

    private void ArrangeTeamGrid(List<Follower> followers, Vector3 centerPosition, Vector2 halfZoneSize)
    {
        int count = followers.Count;
        if (count == 0) return;

        int maxCols = Mathf.Max(1, Mathf.FloorToInt(halfZoneSize.x / followerSpacing));
        int columns = Mathf.Min(count, maxCols);
        int rows = Mathf.CeilToInt((float)count / columns);

        float gridWidth = (columns - 1) * followerSpacing;
        float gridHeight = (rows - 1) * followerSpacing;
        Vector3 startPos = centerPosition - new Vector3(gridWidth / 2f, gridHeight / 2f, 0f);

        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 pos = startPos + new Vector3(col * followerSpacing, row * followerSpacing, 0f);

            pos.x = Mathf.Clamp(pos.x, centerPosition.x - halfZoneSize.x, centerPosition.x + halfZoneSize.x);
            pos.y = Mathf.Clamp(pos.y, centerPosition.y - halfZoneSize.y, centerPosition.y + halfZoneSize.y);

            followers[i].transform.position = pos;
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
                Debug.Log($"⚔️ COMBAT STARTED! P1: {player1Followers.Count} vs P2: {player2Followers.Count}");

            if (combatAnimation != null)
                combatAnimation.SetActive(true);
        }
        else if (p1Has || p2Has)
        {
            currentState = ZoneState.Capturing;
            owner = GetCapturingPlayer();

            if (combatAnimation != null)
                combatAnimation.SetActive(false);
        }
        else
        {
            currentState = ZoneState.Neutral;
            owner = PlayerId.None;

            if (combatAnimation != null)
                combatAnimation.SetActive(false);
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
        //GameManager.OnZoneLocked?.Invoke();
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

        if (combatAnimation != null)
            combatAnimation.SetActive(false);

        if (showCombatLogs)
            Debug.Log($"★★★ {winner} WINS THE BATTLE! ★★★\n");
    }

    // =======================
    // Public Getters
    // =======================
    public ZoneState GetCurrentState() => currentState;
    public PlayerId GetOwner() => owner;
    public float GetAccumulatedPoints(PlayerId player) => accumulatedPoints[player];
}
