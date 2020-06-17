/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;
using System.Linq;
using System;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Message/ChatMulticast")]
    public class Message_ChatMulticast : ANetMessage
    {
        public override void Client_ReceiveMessage(ByteStream data, LLClient client)
        {
            try
            {
                string msg = data.PopString();
                int sender = data.PopInt32();
                client.AddMessageToQueue($"<<< TEAM [{client.NetUsers[sender].UserName}] {msg}");
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @ChatMulticast: [{e.Message}]");
            }
        }

        public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
        {
                data.Append(connectionId);
                // Get all teammates
                int[] targets = server.NetUsers.Where(x => x.Value.TeamNumber == server.NetUsers[connectionId].TeamNumber)
                                        .Select(x => x.Key)
                                        .ToArray();
                server.MulticastNetMessage(targets, server.ReliableChannel, data.ToArray(), connectionId);
        }
    }
}
