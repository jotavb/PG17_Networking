﻿/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using GameSavvy.Byterizer;
using UnityEngine;
using System;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Message/UserInfo")]
    public class Message_UserInfo : ANetMessage
    {
        public override void Client_ReceiveMessage(ByteStream data, LLClient client)
        {
            try 
            {
                int conId = data.PopInt32();
                string userName = data.PopString();
                int teamNum = data.PopInt32();

                var newUser = new NetUser()
                {
                    ConnectionID = conId,
                    UserName = userName,
                    TeamNumber = teamNum
                };
                client.NetUsers[conId] = newUser;
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @OnUserInfo: [{e.Message}]");
            }
        }

        public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
        {
            try
            {
                // Update Current user Data
                var meUser = server.NetUsers[connectionId];
                meUser.UserName = data.PopString();
                meUser.TeamNumber = data.PopInt32();

                ByteStream msg = new ByteStream();
                msg.Encode
                (
                    (byte)NetMessageType.USER_INFO,
                    connectionId,
                    meUser.UserName,    
                    meUser.TeamNumber
                );

                // Broadcast this user info to other users
                server.BroadcastNetMessage(server.ReliableChannel, msg.ToArray(), connectionId);
                
                // Send currently connect users data to this user
                foreach(var user in server.NetUsers)
                {
                    if(user.Key == connectionId) continue;
                    msg = new ByteStream();
                    msg.Encode
                    (
                        (byte)NetMessageType.USER_INFO,
                        user.Key,
                        user.Value.UserName,
                        user.Value.TeamNumber

                    );
                    server.SendNetMessage(connectionId, server.ReliableChannel, msg.ToArray());
                }
                Debug.Log($"@Server -> @UserInfo: User [ID: {connectionId} , UN:{meUser.UserName} , T:{meUser.TeamNumber} ] Registered");
            }
            catch(Exception e)
            {
                Debug.LogError($"@Server -> @OnUserInfo: [{e.Message}]");
            }
        }
    }
}
