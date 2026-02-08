using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Add DOTween namespace

public class DestroyableObject : MonoBehaviour, IDestroyable, IGrantFollowers
{
    [Header("Destroyable Settings")]
    public int maxHealth = 3;                   
    public FollowerStats followerType;          
    public int baseFollowerCount = 1;           
    public int extraFollowersPerHealth = 1;     

    [Header("Visual Effects")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.3f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float popScaleMultiplier = 1.3f;
    [SerializeField] private float popDuration = 0.3f;
    [SerializeField] private Ease popEase = Ease.OutBack;

    private float currentHealth;
    private readonly List<HeroController> attackers = new();
    private readonly Dictionary<HeroController, float> damageContributions = new();
    
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isShaking = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        originalPosition = transform.position;
        originalScale = transform.localScale;
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

            // Trigger shake effect when taking damage
            if (!isShaking)
            {
                PlayShakeEffect();
            }

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                GrantFollowersProportionally();
                PlayDestroyEffect(); // Pop effect before destroying
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
        {
            attackers.Remove(hero);
            
            // Stop shaking when no attackers
            if (attackers.Count == 0)
            {
                StopShakeEffect();
            }
        }
    }

    // ======================
    // VISUAL EFFECTS
    // ======================
    
    private void PlayShakeEffect()
    {
        if (isShaking) return;
        
        isShaking = true;
        
        // Kill any existing tweens on this object
        transform.DOKill();
        
        // Shake the position
        transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 90, false, true)
            .SetLoops(-1, LoopType.Restart) // Loop infinitely while being attacked
            .SetEase(Ease.Linear)
            .OnKill(() => {
                // Return to original position when shake stops
                transform.position = originalPosition;
                isShaking = false;
            });
    }
    
    private void StopShakeEffect()
    {
        if (!isShaking) return;
        
        // Stop shaking
        transform.DOKill();
        transform.position = originalPosition;
        isShaking = false;
    }
    
    private void PlayDestroyEffect()
    {
        // Kill shake effect
        transform.DOKill();
        
        // Reset position
        transform.position = originalPosition;
        
        // Pop effect: scale up then down
        Sequence destroySequence = DOTween.Sequence();
        
        destroySequence.Append(
            transform.DOScale(originalScale * popScaleMultiplier, popDuration * 0.5f)
                .SetEase(popEase)
        );
        
        destroySequence.Append(
            transform.DOScale(Vector3.zero, popDuration * 0.5f)
                .SetEase(Ease.InBack)
        );
        
        // Optional: Add rotation for extra juice
        destroySequence.Join(
            transform.DORotate(new Vector3(0, 0, 360), popDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad)
        );
        
        // Destroy after animation completes
        destroySequence.OnComplete(() => {
            Destroy(gameObject);
        });
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
    
    private void OnDestroy()
    {
        // Clean up any running tweens when object is destroyed
        transform.DOKill();
    }
}