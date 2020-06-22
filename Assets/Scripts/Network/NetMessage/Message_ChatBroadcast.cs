/*
    Copyright (C) Vitor Brito, 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;
using System;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Message/ChatBroadcast")]
    public class Message_ChatBroadcast : ANetMessage
    {
        public override void Client_ReceiveMessage(ByteStream data, LLClient client)
        {
            try
            {
                string msg = data.PopString();
                int sender = data.PopInt32();
                client.AddMessageToQueue($"<<< ALL [{client.NetUsers[sender].UserName}] {msg}");
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @ChatBroadcast: [{e.Message}]");
            }
        }

        public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
        {
            try
            {
                data.Append(connectionId);
                server.BroadcastNetMessage(server.ReliableChannel, data.ToArray(), connectionId);
            }
            catch(Exception e)
            {
                Debug.LogError($"@Server -> @ChatBroadcast: [{e.Message}]");
            }
        }
    }
}
