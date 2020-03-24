/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/

using GameSavvy.Byterizer;
using LLNet;
using UnityEngine;

[CreateAssetMenu(menuName = "LLNet/Message/ConnectionAck")]
public class Message_ConnectionAck : ANetMessage
{
    public override void Client_ReceiveMessage(ByteStream data, LLClient client)
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
        client.SendNetMessage(client.ReliableChannel, data.ToArray());
    }

    public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
    {
        
    }
}
