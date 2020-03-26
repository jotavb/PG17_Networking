/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    public abstract class ANetMessage : ScriptableObject
    {
        [SerializeField] protected NetMessageType _messageType;
        public NetMessageType MessageType => _messageType;

        public abstract void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server);
        public abstract void Client_ReceiveMessage(ByteStream data, LLClient client);
    }
}
