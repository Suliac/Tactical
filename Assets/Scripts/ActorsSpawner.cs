using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct ActorsSpawningInfo
{
    public GameObject Actor;
    public Vector3 SpawnPos;
    public Vector3 FacingDir;
}

public class ActorsSpawner : MonoBehaviour
{
    public List<ActorsSpawningInfo> ActorSpawningInfo = new List<ActorsSpawningInfo>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (var actor in ActorSpawningInfo)
        {
            Instantiate(actor.Actor, actor.SpawnPos, Quaternion.LookRotation(actor.FacingDir));
        }
    }
}
