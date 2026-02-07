using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : MonoBehaviour, IDestroyable, IGrantFollowers
{
    [Header("Destroyable Settings")]
    public int maxHealth = 3;                   
    public FollowerStats followerType;          
    public int baseFollowerCount = 1;           
    public int extraFollowersPerHealth = 1;     

    private float currentHealth;
    private readonly List<HeroController> attackers = new();
    private readonly Dictionary<HeroController, float> damageContributions = new();

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (attackers.Count == 0 || currentHealth <= 0f) return;

        foreach (var hero in attackers)
        {
            float damage = hero.GetDestructionRate() * Time.deltaTime;
            if (damage <= 0f) continue;

            currentHealth -= damage;

            // Track damage contribution
            if (!damageContributions.ContainsKey(hero))
                damageContributions[hero] = 0f;
            damageContributions[hero] += damage;

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                GrantFollowersProportionally();
                Destroy(gameObject);
                break; // Stop processing other attackers
            }
        }
    }

    public void StartBeingAttacked(HeroController hero)
    {
        if (!attackers.Contains(hero))
            attackers.Add(hero);
    }

    public void StopBeingAttacked(HeroController hero)
    {
        if (attackers.Contains(hero))
            attackers.Remove(hero);
    }

    // ======================
    // Grant followers based on damage
    // ======================
    private void GrantFollowersProportionally()
    {
        int totalFollowers = baseFollowerCount + (maxHealth - 1) * extraFollowersPerHealth;

        // Sum total damage
        float totalDamage = 0f;
        foreach (var dmg in damageContributions.Values)
            totalDamage += dmg;

        if (totalDamage <= 0f) return; // Safety

        // Give followers proportional to damage
        foreach (var kvp in damageContributions)
        {
            HeroController hero = kvp.Key;
            float heroDamage = kvp.Value;

            int heroFollowers = Mathf.RoundToInt(totalFollowers * (heroDamage / totalDamage));
            if (heroFollowers > 0)
            {
                hero.GrantFollowers(followerType, heroFollowers);
                Debug.Log($"{hero.name} receives {heroFollowers} followers from destroying {name}");
            }
        }
    }

    public void GrantFollowersToHero(HeroController hero)
    {
        
    }
}
