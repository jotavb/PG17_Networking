/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
namespace LLNet
{
    public enum NetMessageType : byte
    {
        CONNECTION_ACK,
        USER_INFO,
        CHAT_WHISPER,
        CHAT_BROADCAST,
        CHAT_MULTICAST,
        USER_DISCONNECT
    }
}
