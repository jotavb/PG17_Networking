/*
    Copyright (C) Vitor Brito, 2020
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
    public class LLServer : MonoBehaviour
    {
        [SerializeField] private int _serverPort = 27000;
        [SerializeField] private int _bufferSize = 1024;
        [SerializeField] private byte _threadPoolSize = 3;
        [SerializeField] private NetMessageContainer _netMessages;

        private int _socketId;
        [HideInInspector] public byte ReliableChannel { get; private set; }
        [HideInInspector] public byte UnreliableChannel { get; private set; }
        [HideInInspector] public Dictionary<int, NetUser> NetUsers { get; private set; }
        [HideInInspector] public Dictionary<int, NetPlayer> PlayerInstances { get; private set; }

        private void Start()
        {
            _netMessages.MapMessage();
            StartServer();
        }

        private void StartServer()
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
            _socketId = NetworkTransport.AddHost(hostTopology, _serverPort);

            StartCoroutine(Receiver());
            Debug.Log($"StartServer -> {_socketId}");
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
                    Debug.Log($"Error ID -> {error}");
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
                            OnDataReceived(recConnectionId, recChannelId, recBuffer, recDataSize);              
                            break;
                        }
                        case NetworkEventType.ConnectEvent:
                        {
                            OnConnectedToServer(recConnectionId);
                            break;
                        }
                        case NetworkEventType.DisconnectEvent:
                        {
                            OnDisconnected(recSocketId, recConnectionId);
                            break;
                        }
                        default:
                        {
                            Debug.LogWarning($"Receiver -> Unrecognized Message Type [{netEventType.ToString()}]");
                            break;
                        }
                    }
                    if(error != 0)
                    {
                        Debug.Log($"Error ID -> {error}");
                    }
                }
            }        
        }

        private void OnDisconnected(int socketId, int connectionId)
        {
            if(NetUsers.ContainsKey(connectionId) == false) 
            {
                Debug.LogError($"@OnDisconnected -> Try to remove userId[{connectionId}] but it doesn't exist");
                return;
            }
            // Remove user and user's instance
            NetUsers.Remove(connectionId);
            NetPlayer pInstance = PlayerInstances[connectionId];
            PlayerInstances.Remove(connectionId);
            Destroy(pInstance.gameObject);
            // Notify all users about thje disconnection
            ByteStream stream = new ByteStream();
            stream.Encode
            (
                (byte)NetMessageType.USER_DISCONNECT,
                connectionId
            );

            BroadcastNetMessage(ReliableChannel, stream.ToArray(), connectionId);
            Debug.Log($"receiver.Disconnect -> Socket[{socketId}], userId[{connectionId}]");
        }

        private void OnConnectedToServer(int connectionId)
        {
            if(NetUsers.ContainsKey(connectionId))
            {
                Debug.Log($"@OnConnectedToServer -> userId [{connectionId}] has Re-Connected");
            }
            else
            {
                NetUser newUser = new NetUser()
                {
                    ConnectionID = connectionId
                };
                NetUsers[connectionId] = newUser;
                Debug.Log($"@OnConnectedToServer -> UserId[{connectionId}]");
            }
            // Send CONNECTION_ACK with the connectionId to the client
            ByteStream bytestream = new ByteStream();
            bytestream.Append((byte)NetMessageType.CONNECTION_ACK);
            bytestream.Append(connectionId);
            SendNetMessage(connectionId, ReliableChannel, bytestream.ToArray());
            // Spawn a player instance that represents the new user
            PlayerInstances[connectionId] = NetSpawner.SpawnPlayer(Vector3.zero, Quaternion.identity);
        }

        private void OnDataReceived(int connectionId, int channel, byte[] data, int dataSize)
        {
            try
            {
                ByteStream stream = new ByteStream(data, dataSize);
                NetMessageType msgType = (NetMessageType)stream.PopByte();
                _netMessages.NetMessagesMap[msgType].Server_ReceiveMessage(connectionId, stream, this);
            }
            catch(Exception e)
            {
                Debug.LogError($"@Server -> @OnDataReceived: [{e.Message}]");
            }
        }

        public void BroadcastNetMessage(byte channel, byte[] data, int? excludeId = null)
        {
            foreach(var user in NetUsers)
            {
                if(excludeId != null && user.Key == excludeId) continue;
                NetworkTransport.Send
                (
                    _socketId,
                    user.Key,
                    channel,
                    data,
                    data.Length,
                    out var error
                );

                if(error != 0)
                {
                    Debug.LogError($"@Server [{error}] -> Could not send Broadcast to [{user.Key}]");
                }
            }
        }

        public void MulticastNetMessage(int[] targets, byte channel, byte[] data, int? excludeId = null)
        {
            foreach(var user in NetUsers)
            {
                if(excludeId != null && user.Key == excludeId) continue;
                //if(Array.Exists(targets, element => element != user.Key)) continue;
                NetworkTransport.Send
                (
                    _socketId,
                    user.Key,
                    channel,
                    data,
                    data.Length,
                    out var error
                );

                if(error != 0)
                {
                    Debug.LogError($"@Server [{error}] -> Could not send Multicast to [{user.Key}]");
                }
            }
        }
        
        public void SendNetMessage(int targetId, byte channel, byte[] data)
        {
            NetworkTransport.Send
            (
                _socketId,
                targetId,
                channel,
                data,
                data.Length,
                out var error
            );

            if(error != 0)
            {
                Debug.LogError($"@Server -> [{error}] : Could not send message to [{targetId}]");
            }
        }

        #region GUI
            
        private void OnGUI()
        {
            GUILayout.Space(32);
            GUILayout.Label("Online Users:");
            GUILayout.Space(32);

            foreach(var user in NetUsers)
            {
                if(GUILayout.Button($"{user.Key} - {user.Value.UserName}"))
                {
                    // Kick Player
                    NetworkTransport.Disconnect(_socketId, user.Key, out var error);
                }
            }
        }

        #endregion
    }
}