using UnityEngine;using UnityEngine;

[CreateAssetMenu(menuName = "Game/Follower Stats")]
public class FollowerStats : ScriptableObject
{
    public string followerName;
    public float captureSpeed;
    public float maxHealth;
    public float damage;
}