/*
    Copyright (C) Vitor Brito, 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;
using System;
using LLNet;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Message/ConnectionAck")]
    public class Message_ConnectionAck : ANetMessage
    {
        public override void Client_ReceiveMessage(ByteStream data, LLClient client)
        {
            try
            {
                int conId = data.PopInt32();
                NetUser meUser = new NetUser()
                {
                    ConnectionID = conId,
                    UserName = client.UserName,
                    TeamNumber = client.TeamNumber
                };
                client.NetUsers[conId] = meUser;
                client.MyConnectionId = conId;

                data = new ByteStream();
                data.Encode
                (
                    (byte)NetMessageType.USER_INFO,
                    meUser.UserName,
                    meUser.TeamNumber
                );
                // Send USER_INFO message
                client.SendNetMessage(client.ReliableChannel, data.ToArray());
                // Spawn own player instance and flag it
                client.PlayerInstances[meUser.ConnectionID] = NetSpawner.SpawnPlayer(Vector3.zero, Quaternion.identity);
                client.PlayerInstances[meUser.ConnectionID].IsMine = true;
                // Inject the client
                client.PlayerInstances[meUser.ConnectionID].SetClient(client);
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @ConnectionAck: [{e.Message}]");
            }
        }

        public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
        {
            // Only Client
        }
    }
}
