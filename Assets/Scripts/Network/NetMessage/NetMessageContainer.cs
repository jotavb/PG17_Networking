/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/NetMessageContainer")]
    public class NetMessageContainer : ScriptableObject
    {
        [SerializeField] private ANetMessage[] _NetMessages;

        public Dictionary<NetMessageType, ANetMessage> NetMessageMap { get; private set; }

        private void OnValidate()
        {
            MapMessage();
        }

        public void MapMessage()
        {
            NetMessageMap = new Dictionary<NetMessageType, ANetMessage>(_NetMessages.Length);
            foreach(var item in _NetMessages)
            {
                if(item == null ||  NetMessageMap.ContainsKey(item.MessageType))
                {
                    Debug.LogWarning($"Cannot Add Duplicate Message [{item}], item");
                }
                else
                {
                    NetMessageMap[item.MessageType] = item;
                    Debug.Log($"Mapping Done -> Added []");
                }
            }
        }
    }    
}
