/*
    Copyright (C) Vitor Brito, 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;
using System;


namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Message/GameplaySetDestination")]
    public class Message_GameplaySetDestination : ANetMessage
    {
        public override void Client_ReceiveMessage(ByteStream data, LLClient client)
        {
            try
            {
                // Get the destination vector and set on the right player instance
                int conId = data.PopInt32();
                Vector3 destination = data.PopVector3();
                client.PlayerInstances[conId].SetDestination(destination);
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @ConnectionAck: [{e.Message}]");
            }
        }

        public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
        {
            try
            {
                // Get the destination vector and set on the right player instance
                Vector3 destination = data.PopVector3();
                server.PlayerInstances[connectionId].SetDestination(destination);

                data = new ByteStream();
                data.Encode
                (
                    (byte)NetMessageType.GAMEPLAY_SETDESTINATION,
                    connectionId,
                    destination
                );
                // Broadcast this user info to other users
                server.BroadcastNetMessage(server.ReliableChannel, data.ToArray(), connectionId);
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @ConnectionAck: [{e.Message}]");
            }
        }
    }
}
