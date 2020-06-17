/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;
using System;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Message/UserInfoSpawnPosition")]
    public class Message_UserInfo_SpawnPosition : ANetMessage
    {
        public override void Client_ReceiveMessage(ByteStream data, LLClient client)
        {
            try 
            {
                int conId = data.PopInt32();
                string userName = data.PopString();
                int teamNum = data.PopInt32();
                Vector3 pos = data.PopVector3();
                Quaternion rot = data.PopQuaternion();

                var newUser = new NetUser()
                {
                    ConnectionID = conId,
                    UserName = userName,
                    TeamNumber = teamNum
                };
                client.NetUsers[newUser.ConnectionID] = newUser;
                client.PlayerInstances[newUser.ConnectionID] = NetSpawner.SpawnPlayer(pos, rot);
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @OnUserInfo_SpawnPosition: [{e.Message}]");
            }
        }

        public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
        {
            // Only Client - Set the spawn position for the players connected before thee current client
        }
    }
}
