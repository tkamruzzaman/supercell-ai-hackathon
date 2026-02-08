using UnityEngine;

public interface IDestroyable
{
    void StartBeingAttacked(HeroController hero);
    void StopBeingAttacked(HeroController hero);
}