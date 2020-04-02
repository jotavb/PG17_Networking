/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using System.Collections.Generic;
using UnityEngine;

namespace LLNet
{
    public class NetSpawner : MonoBehaviour
    {
        // Spawn and register player instance to the right user
        public static NetPlayer SpawnPlayer(Vector3 pos, Quaternion rot, Transform parent = null)
        {
            GameObject PlayerPrefab = Resources.Load<GameObject>("Player");
            GameObject go = Instantiate(PlayerPrefab, pos, rot, parent);
            return go.GetComponent<NetPlayer>();
        }
    }
}
