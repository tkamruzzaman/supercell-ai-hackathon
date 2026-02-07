using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerId = Enums.PlayerId;
using ZoneState = Enums.ZoneState;

public class CaptureZone : MonoBehaviour
{
    public event System.Action<PlayerId> OnContestResolved;
    public event System.Action<PlayerId, float> OnCapturePointsGenerated;

    public ZoneState GetCurrentState() => currentState;
    public PlayerId GetOwner() => owner;
    public float GetAccumulatedPoints(PlayerId player) => accumulatedPoints[player];
    [Header("Debug")]
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
    [SerializeField] private float followerRadius = 1.5f;

    private void Update()
    {
        TickCapture();
        HandleCombat();
        ArrangeFollowers();
        //DebugInput();
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
        List<Follower> followers = GetFollowers(player);
        foreach (var f in followers)
            total += f.GetStats().captureSpeed;
        return total;
    }

    // =======================
    // Combat Logic
    // =======================
    private void HandleCombat()
    {
        if (currentState != ZoneState.Contested) return;

        // Player1 attacks Player2
        for (int i = player1Followers.Count - 1; i >= 0; i--)
        {
            if (player2Followers.Count == 0) break;
            Follower attacker = player1Followers[i];
            Follower target = player2Followers[Random.Range(0, player2Followers.Count)];
            target.TakeDamage(attacker.GetStats().damage * Time.deltaTime);

            if (target.IsDead())
            {
                player2Followers.Remove(target);
                Destroy(target.gameObject);
            }
        }

        // Player2 attacks Player1
        for (int i = player2Followers.Count - 1; i >= 0; i--)
        {
            if (player1Followers.Count == 0) break;
            Follower attacker = player2Followers[i];
            Follower target = player1Followers[Random.Range(0, player1Followers.Count)];
            target.TakeDamage(attacker.GetStats().damage * Time.deltaTime);

            if (target.IsDead())
            {
                player1Followers.Remove(target);
                Destroy(target.gameObject);
            }
        }

        // Resolve contest if one side wiped
        if (player1Followers.Count == 0 && player2Followers.Count > 0) ResolveContest(PlayerId.Player2);
        else if (player2Followers.Count == 0 && player1Followers.Count > 0) ResolveContest(PlayerId.Player1);
    }

    // =======================
    // Deposit API
    // =======================
    public bool TryDepositFollower(PlayerId player, Follower follower)
    {
        if (currentState == ZoneState.Locked || follower == null) return false;

        follower.Detach();
        follower.InitializeHealth(follower.GetStats().maxHealth);

        if (player == PlayerId.Player1) player1Followers.Add(follower);
        else player2Followers.Add(follower);

        UpdateZoneState();
        return true;
    }

    private void ArrangeFollowers()
    {
        Arrange(player1Followers, 0f);
        Arrange(player2Followers, Mathf.PI);
    }

    private void Arrange(List<Follower> followers, float angleOffset)
    {
        int count = followers.Count;
        for (int i = 0; i < count; i++)
        {
            float angle = 2 * Mathf.PI * i / count + angleOffset;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * followerRadius;
            followers[i].transform.position = transform.position + offset;
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
        Debug.Log($"[CaptureZone] Locked by {winner}");
    }

    public void ResolveContest(PlayerId winner)
    {
        if (winner == PlayerId.Player1) player2Followers.Clear();
        else if (winner == PlayerId.Player2) player1Followers.Clear();

        currentState = ZoneState.Capturing;
        owner = winner;
        controlPercent = 0f;
        OnContestResolved?.Invoke(winner);
    }

    // =======================
    // Debug
    // =======================
    [Header("Debug Deposit")]
    [SerializeField] private FollowerStats scoutStats;
    [SerializeField] private FollowerStats tankStats;

    // private void DebugInput()
    // {
    //     if (Keyboard.current == null) return;
    //
    //     if (Keyboard.current.digit1Key.wasPressedThisFrame)
    //         TryDepositFollower(PlayerId.Player1, CreateDebugFollower(scoutStats));
    //
    //     if (Keyboard.current.digit2Key.wasPressedThisFrame)
    //         TryDepositFollower(PlayerId.Player2, CreateDebugFollower(tankStats));
    //
    //     if (Keyboard.current.kKey.wasPressedThisFrame)
    //         Debug.Log($"State: {currentState}, Owner: {owner}, Control: {controlPercent:F1}%");
    // }
    //
    // private Follower CreateDebugFollower(FollowerStats stats)
    // {
    //     GameObject go = Instantiate(stats.prefab, transform.position, Quaternion.identity);
    //     Follower f = go.GetComponent<Follower>();
    //     f.Detach();
    //     f.InitializeHealth(stats.maxHealth);
    //     return f;
    // }
}
