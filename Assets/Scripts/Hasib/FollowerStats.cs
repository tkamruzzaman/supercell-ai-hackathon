using UnityEngine;

[CreateAssetMenu(menuName = "Game/Follower Stats")]
public class FollowerStats : ScriptableObject
{
    public string followerName;

    [Header("Gameplay")]
    public float captureSpeed;
    public float maxHealth;
    public float damage;

    [Header("Spawn")]
    public GameObject prefab;  // assign prefab in Inspector
}