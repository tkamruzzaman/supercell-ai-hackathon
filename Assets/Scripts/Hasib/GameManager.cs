using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // TextMeshPro namespace
using PlayerId = Enums.PlayerId;
using ZoneState = Enums.ZoneState;

public class GameManager : MonoBehaviour
{
    public static event Action OnMatchStart;
    public static event Action OnZoneLocked;
    [Header("Match Settings")]
    [SerializeField] private float matchDuration = 180f;
    
    public int playerCount = 0;
    [Header("Capture Zones")]
    [SerializeField] private List<CaptureZone> captureZones;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;         // Timer display
    [SerializeField] private TextMeshProUGUI winnerText;        // Winner display
    [SerializeField] private GameObject startUI;                // UI for waiting players
    [SerializeField] private GameObject endUI;                  // UI for end match

    [Header("Debug")]
    [SerializeField] private bool debugEndMatchEarly = false;

    private float timer;
    private bool matchEnded = false;
    private bool matchStarted = false;

    // Points for tie-breaker (from capture progress)
    private Dictionary<PlayerId, float> playerPoints = new()
    {
        { PlayerId.Player1, 0f },
        { PlayerId.Player2, 0f }
    };

    // Track if players joined
    private Dictionary<PlayerId, bool> playerJoined = new()
    {
        { PlayerId.Player1, false },
        { PlayerId.Player2, false }
    };

    private void OnEnable()
    {
        
        OnZoneLocked += CheckAllZonesLocked;
    }
    private void OnDisable()
    {
        
        OnZoneLocked -= CheckAllZonesLocked;
    }

    public void AddPlayer()
    {
        playerCount++;
    }

    private void Start()
    {
        timer = matchDuration;
        matchEnded = false;
        matchStarted = false;

        startUI.SetActive(true);
        endUI.SetActive(false);

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

        if (!matchStarted)
        {
            if (playerCount>1)
                StartMatch();
            return;
        }

        if (matchEnded) return;

        timer -= Time.deltaTime;
        UpdateTimerUI();

        if (timer <= 0f || debugEndMatchEarly)
            EndMatch();
        
        if (AllZonesLocked())
            EndMatch();
    }
    
    public void CheckAllZonesLocked()
    {
        if (matchEnded) return;

        if (AllZonesLocked())
        {
            Debug.Log("[GameManager] All zones locked! Ending match early.");
            EndMatch();
        }
    }
    
    private bool AllZonesLocked()
    {
        foreach (var zone in captureZones)
        {
            if (zone.GetCurrentState() != ZoneState.Locked)
                return false;
        }
        return true;
    }

    // ==============================
    // Player Join
    // ==============================
    public void PlayerJoin(PlayerId player)
    {
        playerJoined[player] = true;
        Debug.Log($"[GameManager] {player} joined the match!");
    }

    private void StartMatch()
    {
        matchStarted = true;
        OnMatchStart?.Invoke();
        startUI.SetActive(false);
        Debug.Log("[GameManager] Match Started!");
    }

    // ==============================
    // UI Updates
    // ==============================
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void ShowWinnerUI(PlayerId winner)
    {
        if (winnerText != null)
        {
            if (winner == PlayerId.None)
            {
                winnerText.text = "Perfect Tie!";
            }

            else
            {
                string player;
                if (winner == PlayerId.Player1)
                {
                    player = winner.ToString()+ " (Red)";
                }
                else
                {
                    player = winner.ToString() + " (Blue)";
                }
                winnerText.text = $"{player} Wins!";
            }
               
        }

        if (endUI != null)
            endUI.SetActive(true);
    }

    // ==============================
    // ZONE EVENT HANDLERS
    // ==============================
    private void OnZoneContestResolved(PlayerId winner)
    {
        Debug.Log($"[GameManager] Zone contest resolved. Winner: {winner}");
    }

    private void OnZonePointsGenerated(PlayerId player, float points)
    {
        if (matchEnded) return;

        if (playerPoints.ContainsKey(player))
            playerPoints[player] += points;
    }

    // ==============================
    // MATCH END LOGIC
    // ==============================
    private void EndMatch()
    {
        matchEnded = true;
        playerCount = 0;
        Dictionary<PlayerId, int> lockedCounts = new()
        {
            { PlayerId.Player1, 0 },
            { PlayerId.Player2, 0 }
        };

        playerPoints[PlayerId.Player1] = 0f;
        playerPoints[PlayerId.Player2] = 0f;

        foreach (var zone in captureZones)
        {
            var state = zone.GetCurrentState();
            var owner = zone.GetOwner();

            if (state == ZoneState.Locked)
                lockedCounts[owner]++;

            playerPoints[PlayerId.Player1] += zone.GetAccumulatedPoints(PlayerId.Player1);
            playerPoints[PlayerId.Player2] += zone.GetAccumulatedPoints(PlayerId.Player2);
        }

        PlayerId winner = PlayerId.None;

        if (lockedCounts[PlayerId.Player1] > lockedCounts[PlayerId.Player2])
            winner = PlayerId.Player1;
        else if (lockedCounts[PlayerId.Player2] > lockedCounts[PlayerId.Player1])
            winner = PlayerId.Player2;
        else
        {
            if (playerPoints[PlayerId.Player1] > playerPoints[PlayerId.Player2])
                winner = PlayerId.Player1;
            else if (playerPoints[PlayerId.Player2] > playerPoints[PlayerId.Player1])
                winner = PlayerId.Player2;
            else
                winner = PlayerId.None;
        }

        Debug.Log($"[GameManager] Locked zones - P1: {lockedCounts[PlayerId.Player1]}, P2: {lockedCounts[PlayerId.Player2]}");
        Debug.Log($"[GameManager] Points - P1: {playerPoints[PlayerId.Player1]:F1}, P2: {playerPoints[PlayerId.Player2]:F1}");

        ShowWinnerUI(winner);
        Debug.Log(winner != PlayerId.None ? $"[GameManager] Winner: {winner}" : "[GameManager] Perfect tie!");
    }

    // ==============================
    // DEBUG FUNCTION
    // ==============================
    private void DebugInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
            Debug.Log("Current timer: " + timer);
    }

    public void ForceEndMatch()
    {
        if (!matchEnded)
            EndMatch();
    }
}
