/*
    Copyright (C) Vitor Brito, 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;
using System;
using LLNet;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Message/ChatWhisper")]
    public class Message_ChatWhisper : ANetMessage
    {
        public override void Client_ReceiveMessage(ByteStream data, LLClient client)
        {
            try
            {
                data.PopInt32();
                string msg = data.PopString();
                int sender = data.PopInt32();
                client.AddMessageToQueue($"<<<<<< [{client.NetUsers[sender].UserName}] {msg}");
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @ChatWhisper: [{e.Message}]");
            }
        }

        public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
        {
            try
            {
                int targetId = data.PopInt32();
                data.Append(connectionId);
                server.SendNetMessage(targetId, server.ReliableChannel, data.ToArray());
            }
            catch(Exception e)
            {
                Debug.LogError($"@Server -> @ChatWhisper: [{e.Message}]");
            }
        }
    }
}