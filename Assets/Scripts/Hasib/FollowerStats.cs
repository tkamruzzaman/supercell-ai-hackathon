using UnityEngine;

[CreateAssetMenu(menuName = "Game/Follower Stats")]
public class FollowerStats : ScriptableObject
{
    public string followerName;

    [Header("Gameplay")]
    public float captureSpeed;
    public float maxHealth;
    public float damage;
    public float attackSpeed = 1f; // NEW: Attacks per second

    [Header("Spawn Prefabs")]
    public GameObject player1Prefab; // prefab for Player 1
    public GameObject player2Prefab; // prefab for Player 2

    /// <summary>
    /// Returns the prefab corresponding to the player.
    /// </summary>
    public GameObject GetPrefab(Enums.PlayerId player)
    {
        return player == Enums.PlayerId.Player1 ? player1Prefab : player2Prefab;
    }
}