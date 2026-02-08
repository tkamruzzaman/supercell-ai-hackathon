using System.Collections.Generic;
using UnityEngine;

public static class PlayerIdTracker
{
    private static readonly HashSet<Enums.PlayerId> takenIds = new();

    public static Enums.PlayerId GetNextAvailable()
    {
        foreach (Enums.PlayerId id in System.Enum.GetValues(typeof(Enums.PlayerId)))
        {
            if (id == Enums.PlayerId.None) continue; // skip None
            if (!takenIds.Contains(id))
            {
                takenIds.Add(id);
                return id;
            }
        }

        Debug.LogWarning("No available PlayerId!");
        return Enums.PlayerId.None;
    }

    public static void Release(Enums.PlayerId id)
    {
        if (takenIds.Contains(id))
            takenIds.Remove(id);
    }
}