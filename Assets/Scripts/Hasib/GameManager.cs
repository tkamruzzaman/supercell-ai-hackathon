using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerId = Enums.PlayerId;
using ZoneState = Enums.ZoneState;
public class GameManager : MonoBehaviour
{
    public static int count;
    [Header("Match Settings")]
    [SerializeField] private float matchDuration = 180f;
    
    [Header("Capture Zones")]
    [SerializeField] private List<CaptureZone> captureZones;

    [Header("Debug")]
    [SerializeField] private bool debugEndMatchEarly = false;

    private float timer;
    private bool matchEnded = false;

    // Points for tie-breaker (from capture progress)
    private Dictionary<PlayerId, float> playerPoints = new()
    {
        { PlayerId.Player1, 0f },
        { PlayerId.Player2, 0f }
    };

    private void Start()
    {
        timer = matchDuration;
        matchEnded = false;

        // Subscribe to zone events
        foreach (var zone in captureZones)
        {
            zone.OnContestResolved += OnZoneContestResolved;
            zone.OnCapturePointsGenerated += OnZonePointsGenerated;
        }
    }

    private void Update()
    {
        DebugInput();
        if (matchEnded) return;

        timer -= Time.deltaTime;

        if (timer <= 0f || debugEndMatchEarly)
        {
            EndMatch();
        }
    }
    private void DebugInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
            Debug.Log("Current timer: " + timer);
    }

    // ==============================
    // ZONE EVENT HANDLERS
    // ==============================

    private void OnZoneContestResolved(PlayerId winner)
    {
        Debug.Log($"[GameManager] Zone contest resolved. Winner: {winner}");
        // Optional: you can play sounds, animations, etc. here
    }

    private void OnZonePointsGenerated(PlayerId player, float points)
    {
        if (matchEnded) return;

        if (playerPoints.ContainsKey(player))
        {
            playerPoints[player] += points;
        }
    }

    // ==============================
    // MATCH END LOGIC
    // ==============================

    private void EndMatch()
    {
        matchEnded = true;

        // Count locked zones per player
        Dictionary<PlayerId, int> lockedCounts = new()
        {
            { PlayerId.Player1, 0 },
            { PlayerId.Player2, 0 }
        };

        // Reset player points
        playerPoints[PlayerId.Player1] = 0f;
        playerPoints[PlayerId.Player2] = 0f;

        // Sum locked zones and accumulated points
        foreach (var zone in captureZones)
        {
            var state = zone.GetCurrentState();
            var owner = zone.GetOwner();

            // Locked zones
            if (state == Enums.ZoneState.Locked)
            {
                lockedCounts[owner]++;
            }

            // Add accumulated points for tie-breaker
            playerPoints[PlayerId.Player1] += zone.GetAccumulatedPoints(PlayerId.Player1);
            playerPoints[PlayerId.Player2] += zone.GetAccumulatedPoints(PlayerId.Player2);
        }

        // Determine winner
        PlayerId winner = PlayerId.None;

        if (lockedCounts[PlayerId.Player1] > lockedCounts[PlayerId.Player2])
            winner = PlayerId.Player1;
        else if (lockedCounts[PlayerId.Player2] > lockedCounts[PlayerId.Player1])
            winner = PlayerId.Player2;
        else
        {
            // Tie in locked zones → use points
            if (playerPoints[PlayerId.Player1] > playerPoints[PlayerId.Player2])
                winner = PlayerId.Player1;
            else if (playerPoints[PlayerId.Player2] > playerPoints[PlayerId.Player1])
                winner = PlayerId.Player2;
            else
                winner = PlayerId.None; // Perfect tie
        }

        // Debug logs
        Debug.Log($"[GameManager] Locked zones - P1: {lockedCounts[PlayerId.Player1]}, P2: {lockedCounts[PlayerId.Player2]}");
        Debug.Log($"[GameManager] Points - P1: {playerPoints[PlayerId.Player1]:F1}, P2: {playerPoints[PlayerId.Player2]:F1}");

        if (winner != PlayerId.None)
            Debug.Log($"[GameManager] Winner: {winner}");
        else
            Debug.Log("[GameManager] Perfect tie!");

        // Optional: disable input, show UI, etc.
    }




    // ==============================
    // DEBUG FUNCTION
    // ==============================
    public void ForceEndMatch()
    {
        if (!matchEnded)
            EndMatch();
    }
}
