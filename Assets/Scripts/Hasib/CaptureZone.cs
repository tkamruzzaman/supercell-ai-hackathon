using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerId = Enums.PlayerId;
using ZoneState = Enums.ZoneState;

public class CaptureZone : MonoBehaviour
{
    public event System.Action<PlayerId> OnContestResolved;
    public event System.Action<PlayerId, float> OnCapturePointsGenerated;

    [Header("Debug")]
    [SerializeField] private ZoneState currentState = ZoneState.Neutral;
    [SerializeField] private PlayerId owner = PlayerId.None;
    [SerializeField, Range(0f, 100f)] private float controlPercent = 0f;

    // Persistent points for tie-breaker
    private Dictionary<PlayerId, float> accumulatedPoints = new()
    {
        { PlayerId.Player1, 0f },
        { PlayerId.Player2, 0f }
    };

    private readonly List<FollowerData> player1Followers = new();
    private readonly List<FollowerData> player2Followers = new();

    private void Update()
    {
        TickCapture();
        DebugInput();
    }

    // ==============================
    // CAPTURE LOGIC
    // ==============================
    private void TickCapture()
    {
        if (currentState == ZoneState.Capturing)
        {
            PlayerId capturingPlayer = GetCapturingPlayer();
            if (capturingPlayer != PlayerId.None)
            {
                float captureSpeed = GetTotalCaptureSpeed(capturingPlayer);
                if (captureSpeed > 0f)
                {
                    float delta = captureSpeed * Time.deltaTime;

                    // Increase zone capture progress
                    controlPercent += delta;

                    // Accumulate points for tie-breaker
                    accumulatedPoints[capturingPlayer] += delta;

                    // Fire GameManager event
                    OnCapturePointsGenerated?.Invoke(capturingPlayer, delta);

                    if (controlPercent >= 100f)
                    {
                        controlPercent = 100f;
                        LockZone(capturingPlayer);
                    }
                }
            }
        }

        if (currentState == ZoneState.Contested)
        {
            // Capture progress stops
            // Do NOT increase controlPercent, but accumulated points remain
        }
    }

    private float GetTotalCaptureSpeed(PlayerId player)
    {
        float total = 0f;
        List<FollowerData> followers = GetFollowers(player);
        foreach (var f in followers)
            total += f.stats.captureSpeed;
        return total;
    }

    // ==============================
    // PUBLIC API
    // ==============================
    public bool TryDepositFollower(PlayerId player, FollowerStats stats)
    {
        if (currentState == ZoneState.Locked)
            return false;

        AddFollower(player, stats);
        UpdateZoneState();
        return true;
    }

    public ZoneState GetCurrentState() => currentState;
    public PlayerId GetOwner() => owner;
    public float GetControlPercent() => controlPercent;

    public float GetAccumulatedPoints(PlayerId player) => accumulatedPoints[player];

    // ==============================
    // INTERNAL
    // ==============================
    private void AddFollower(PlayerId player, FollowerStats stats)
    {
        var follower = new FollowerData(stats);

        if (player == PlayerId.Player1)
            player1Followers.Add(follower);
        else
            player2Followers.Add(follower);
    }

    private void UpdateZoneState()
    {
        bool p1Has = player1Followers.Count > 0;
        bool p2Has = player2Followers.Count > 0;

        if (p1Has && p2Has)
        {
            currentState = ZoneState.Contested;

            // Reset capture progress for zone lock
            controlPercent = 0f;

            // Owner is temporarily none during fight
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
        if (player1Followers.Count > 0 && player2Followers.Count == 0)
            return PlayerId.Player1;

        if (player2Followers.Count > 0 && player1Followers.Count == 0)
            return PlayerId.Player2;

        return PlayerId.None;
    }

    private List<FollowerData> GetFollowers(PlayerId player)
    {
        return player == PlayerId.Player1 ? player1Followers : player2Followers;
    }

    private void LockZone(PlayerId winner)
    {
        currentState = ZoneState.Locked;
        owner = winner;

        // Ensure tie-breaker points reflect full control
        accumulatedPoints[winner] += 100f;

        player1Followers.Clear();
        player2Followers.Clear();

        Debug.Log($"[CaptureZone] Locked by {winner}");
    }

    public void ResolveContest(PlayerId winner)
    {
        if (winner == PlayerId.Player1) player2Followers.Clear();
        else if (winner == PlayerId.Player2) player1Followers.Clear();

        currentState = ZoneState.Capturing;
        owner = winner;

        // Do NOT reset accumulated points
        controlPercent = 0f;

        OnContestResolved?.Invoke(winner);
    }

    // ==============================
    // DEBUG
    // ==============================
    [Header("Debug Deposit")]
    [SerializeField] private FollowerStats scoutStats;
    [SerializeField] private FollowerStats soldierStats;
    [SerializeField] private FollowerStats tankStats;

    private void DebugInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            TryDepositFollower(PlayerId.Player1, scoutStats);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            TryDepositFollower(PlayerId.Player2, tankStats);

        if (Keyboard.current.kKey.wasPressedThisFrame)
            Debug.Log($"State: {currentState}, Owner: {owner}, Control: {controlPercent:F1}%");

        if (Keyboard.current.fKey.wasPressedThisFrame)
            ResolveContest(PlayerId.Player2);
        if (Keyboard.current.rKey.wasPressedThisFrame)
            ResolveContest(PlayerId.Player1);
    }
}
