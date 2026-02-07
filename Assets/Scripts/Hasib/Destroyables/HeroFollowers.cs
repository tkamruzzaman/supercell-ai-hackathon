using System.Collections.Generic;
using UnityEngine;

public class HeroFollowers : MonoBehaviour
{
    private readonly List<Follower> followers = new();

    public bool HasFollowers() => followers.Count > 0;
    public int Count => followers.Count;

    public void AddFollower(Follower follower)
    {
        followers.Add(follower);
        Reindex();
    }

    public Follower RemoveOneFollower()
    {
        if (followers.Count == 0)
            return null;

        Follower f = followers[^1];
        followers.RemoveAt(followers.Count - 1);

        f.Detach();
        Reindex();

        return f;
    }

    private void Reindex()
    {
        for (int i = 0; i < followers.Count; i++)
            followers[i].SetIndex(i, followers.Count);
    }
}