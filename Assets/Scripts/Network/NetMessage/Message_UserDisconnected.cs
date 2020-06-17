/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;
using System;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Message/UserDisconnected")]
    public class Message_UserDisconnected : ANetMessage
    {
        public override void Client_ReceiveMessage(ByteStream data, LLClient client)
        {
            try
            {
                int conId = data.PopInt32();
                if(client.NetUsers.ContainsKey(conId) == false)
                {
                    Debug.LogError("@Client -> @UserDisconnected: Trying to remove an Id that didn't exist");
                    return;
                }
                client.NetUsers.Remove(conId);
                NetPlayer pInstance = client.PlayerInstances[conId];
                client.PlayerInstances.Remove(conId);
                Destroy(pInstance.gameObject);
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @UserDisconnected: [{e.Message}]");
            }
        }

        public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
        {
            // Only Client
        }
    }
}