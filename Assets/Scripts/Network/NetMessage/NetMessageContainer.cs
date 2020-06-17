/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using System.Collections.Generic;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/NetMessageContainer")]
    public class NetMessageContainer : ScriptableObject
    {
        [SerializeField] private ANetMessage[] _NetMessages;

        public Dictionary<NetMessageType, ANetMessage> NetMessagesMap { get; private set; }

        // OnValidate doesn't work on build
        private void OnEnable()
        {
            MapMessage();
        }

        public void MapMessage()
        {
            NetMessagesMap = new Dictionary<NetMessageType, ANetMessage>(_NetMessages.Length);
            foreach(var item in _NetMessages)
            {
                if(item == null ||  NetMessagesMap.ContainsKey(item.MessageType))
                {
                    Debug.LogWarning($"Cannot Add Duplicate Message [{item}], item");
                }
                else
                {
                    NetMessagesMap[item.MessageType] = item;
                    Debug.Log($"Mapping Done -> Added [{NetMessagesMap.Count}] messages!");
                }
            }
        }
    }    
}
