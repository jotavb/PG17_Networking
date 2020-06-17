/*
    Copyright (C) Team Tripple Double, Vitor Brito 2020
*/
using System.Collections.Generic;
using UnityEngine.Networking;
using GameSavvy.Byterizer;
using System.Collections;
using UnityEngine;
using System;

namespace LLNet
{
    [System.Obsolete]
    public class LLClient : MonoBehaviour
    {
        [SerializeField] private string _serverAdress = "127.0.0.1";
        [SerializeField] private int _serverPort = 27000;
        [SerializeField] private int _bufferSize = 1024;
        [SerializeField] private byte _threadPoolSize = 3;
        [SerializeField] private NetMessageContainer _netMessages;

        [Header("User Data")]
        [SerializeField] private string _userName = "JBrito";
        public string UserName => _userName;
        [SerializeField] private int _teamNumber = 1;
        public int TeamNumber => _teamNumber;

        private int _socketId = 0;
        private int _serverConnectionId = 0;

        [HideInInspector] public int MyConnectionId = -1;
        [HideInInspector] public byte ReliableChannel { get; private set; }
        [HideInInspector] public byte UnreliableChannel { get; private set; }

        [HideInInspector] public Dictionary<int, NetUser> NetUsers { get; private set; }
        [HideInInspector] public Dictionary<int, NetPlayer> PlayerInstances { get; private set; }

        private void Start()
        {
            _userName += UnityEngine.Random.Range(1, 1000);
            _netMessages.MapMessage();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            NetUsers = new Dictionary<int, NetUser>();
            PlayerInstances = new Dictionary<int, NetPlayer>();

            GlobalConfig globalconfig = new GlobalConfig()
            {
                ThreadPoolSize = _threadPoolSize
            };

            NetworkTransport.Init(globalconfig);

            ConnectionConfig connectionConfig = new ConnectionConfig()
            {
                SendDelay = 0,
                MinUpdateTimeout = 1
            };

            ReliableChannel = connectionConfig.AddChannel(QosType.Reliable);
            UnreliableChannel = connectionConfig.AddChannel(QosType.Unreliable);

            HostTopology hostTopology = new HostTopology(connectionConfig, 16);
            _socketId = NetworkTransport.AddHost(hostTopology, 0);

            byte error;
            _serverConnectionId = NetworkTransport.Connect(_socketId, _serverAdress, _serverPort, 0, out error);

            if(error != 0)
            {
                Debug.LogError($"@Client -> @ConnectToServer : [{error}]");
            }
            else
            {
                StartCoroutine(Receiver());
                Debug.Log($"@Client -> @ConnectToServer : [{_socketId}]");
            }
        }

        private IEnumerator Receiver()
        {
            int recSocketId, recConnectionId, recChannelId, recDataSize;
            byte error = 0;
            byte[] recBuffer = new byte[_bufferSize];
            
            while(true)
            {
                NetworkEventType netEventType = NetworkTransport.Receive
                (
                    out recSocketId,
                    out recConnectionId,
                    out recChannelId,
                    recBuffer,
                    _bufferSize,
                    out recDataSize,
                    out error
                );

                if(error != 0)
                {
                    Debug.LogError($"@Client -> @Receiver : Error ID [{error}]");
                }
                else
                {
                    switch(netEventType)
                    {
                        case NetworkEventType.Nothing:
                        {
                            yield return null;
                            break;
                        }
                        case NetworkEventType.DataEvent:
                        {
                            OnDataReceived(recChannelId, recBuffer, recDataSize);
                            break;
                        }
                        case NetworkEventType.ConnectEvent:
                        {
                            Debug.Log($"@Client -> Receiver.Connect : Socket[{recSocketId}], userId[{recConnectionId}]");
                            break;
                        }
                        case NetworkEventType.DisconnectEvent:
                        {
                            Debug.Log($"@Client -> Receiver.Disconnect : Socket[{recSocketId}], userId[{recConnectionId}]");
                            Application.Quit();
                            break;
                        }
                        default:
                        {
                            Debug.LogError($"@Client -> @Receiver : Unrecognized Message Type [{netEventType.ToString()}]");
                            break;
                        }
                    }
                }
            }
        }

        private void OnDataReceived(int recChannelId, byte[] data, int dataSize)
        {
            try
            {
                ByteStream stream = new ByteStream(data, dataSize);
                NetMessageType msgType = (NetMessageType)stream.PopByte();
                _netMessages.NetMessagesMap[msgType].Client_ReceiveMessage(stream, this);
            }
            catch(Exception e)
            {
                Debug.LogError($"@Client -> @OnDataReceived: [{e.Message}]");
            }
        }

        public void SendNetMessage(byte channel, byte[] data)
        {
            NetworkTransport.Send
            (
                _socketId,
                _serverConnectionId,
                channel,
                data,
                data.Length,
                out var error
            );

            if(error != 0)
            {
                Debug.LogError($"@Client -> [{error}] : Could not send message to Server");
            }
        }

    #region GUI
    
        private string _msgToSend;
        private Queue<string> _chatMessage = new Queue<string>();
        public void AddMessageToQueue(string msg)
        {
            _chatMessage.Enqueue(msg);
            if(_chatMessage.Count > 16)
            {
                _chatMessage.Dequeue();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(32);
                    _msgToSend = GUILayout.TextField(_msgToSend);
                    GUILayout.Space(32);
                    if(GUILayout.Button("ALL"))
                    {
                        ByteStream stream = new ByteStream();
                        stream.Encode
                        (
                            (byte)NetMessageType.CHAT_BROADCAST,
                            _msgToSend
                        );
                        SendNetMessage(ReliableChannel, stream.ToArray());
                        AddMessageToQueue($"ALL > {_msgToSend}");
                        _msgToSend = "";
                    }
                    GUILayout.Space(32);
                    if(GUILayout.Button("TEAM"))
                    {
                        ByteStream stream = new ByteStream();
                        stream.Encode
                        (
                            (byte)NetMessageType.CHAT_MULTICAST,
                            _msgToSend
                        );
                        SendNetMessage(ReliableChannel, stream.ToArray());
                        AddMessageToQueue($"TEAM > {_msgToSend}");
                        _msgToSend = "";
                    }
                    GUILayout.Space(40);
                    GUILayout.Label("Online Users");
                    GUILayout.Space(32);

                    foreach(var user in NetUsers)
                    {
                        if(GUILayout.Button(user.Value.UserName))
                        {
                            if(user.Key == MyConnectionId)
                            {
                                AddMessageToQueue("Cannot send whisper to yourself.");
                                return;
                            } 
                            ByteStream stream = new ByteStream();
                            stream.Encode
                            (
                                (byte)NetMessageType.CHAT_WHISPER,
                                user.Value.ConnectionID,
                                _msgToSend
                            );
                            SendNetMessage(ReliableChannel, stream.ToArray());
                            AddMessageToQueue($">>>> {_msgToSend}");
                            _msgToSend = "";
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(40);
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Chat Messages:");
                    GUILayout.Space(32);
                    foreach (string msg in _chatMessage)
                    {
                        GUILayout.Label(msg);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    #endregion
    }
}
