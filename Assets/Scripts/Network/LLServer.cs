using GameSavvy.Byterizer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace LLNet
{
    [System.Obsolete]
    public class LLServer : MonoBehaviour
    {
        [SerializeField] private int _serverPort = 27000;
        [SerializeField] private int _bufferSize = 1024;
        [SerializeField] private byte _threadPoolSize = 3;

        private byte _reliableChannel;
        private byte _unreliableChannel;
        private int _socketId;
        private Dictionary<int, NetUser> _NetUsers;

        private void Start()
        {
            StartServer();
        }

        private void StartServer()
        {
             _NetUsers = new Dictionary<int, NetUser>();

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

            _reliableChannel = connectionConfig.AddChannel(QosType.Reliable);
            _unreliableChannel = connectionConfig.AddChannel(QosType.Unreliable);

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
            if(_NetUsers.ContainsKey(connectionId) == false) 
            {
                Debug.LogError($"@OnDisconnected -> Try to remove userId[{connectionId}] but it doesn't exist");
                return;
            }
            _NetUsers.Remove(connectionId);
            ByteStream stream = new ByteStream();
            stream.Encode
            (
                (byte)NetMessageType.USER_DISCONNECT,
                connectionId
            );
            BroadcastNetMessage(_reliableChannel, stream.ToArray(), connectionId);
            Debug.Log($"receiver.Disconnect -> Socket[{socketId}], userId[{connectionId}]");
        }

        private void OnConnectedToServer(int connectionId)
        {
            if(_NetUsers.ContainsKey(connectionId))
            {
                Debug.Log($"@OnConnectedToServer -> userId [{connectionId}] has Re-Connected");
            }
            else
            {
                NetUser newUser = new NetUser()
                {
                    ConnectionID = connectionId
                };
                _NetUsers[connectionId] = newUser;
                Debug.Log($"@OnConnectedToServer -> UserId[{connectionId}]");
            }
            ByteStream bytestream = new ByteStream();
            bytestream.Append((byte)NetMessageType.CONNECTION_ACK);
            bytestream.Append(connectionId);
            SendNetMessage(connectionId, _reliableChannel, bytestream.ToArray());
        }

        private void OnDataReceived(int connectionId, int channel, byte[] data, int dataSize)
        {
            ByteStream byteStream = new ByteStream(data, dataSize);
            NetMessageType msgType = (NetMessageType)byteStream.PopByte();
            switch(msgType)
            {
                case NetMessageType.USER_INFO:
                {
                    OnUserInfo(connectionId, byteStream);
                    break;
                }
                case NetMessageType.CHAT_WHISPER:
                {
                    OnChatWhisper(connectionId, byteStream);
                    break;
                }
                case NetMessageType.CHAT_BROADCAST:
                {
                    OnChatBroadcast(connectionId, byteStream);
                    break;
                }
                case NetMessageType.CHAT_TEAM_MESSAGE:
                {
                    OnChatMulticast(connectionId, byteStream);
                    break;
                }
                default:
                break;
            }
        }

        private void OnUserInfo(int connectionId, ByteStream stream)
        {
            // Update Current user Data
            var meUser = _NetUsers[connectionId];
            meUser.UserName = stream.PopString();
            meUser.TeamNumber = stream.PopInt32();

            ByteStream msg = new ByteStream();
            msg.Encode
            (
                (byte)NetMessageType.USER_INFO,
                connectionId,
                meUser.UserName,
                meUser.TeamNumber
            );

            // Broadcast this user info to other users
            BroadcastNetMessage(_reliableChannel, msg.ToArray(), connectionId);
            
            // Send currently connect users data to this user
            foreach(var user in _NetUsers)
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
                SendNetMessage(connectionId, _reliableChannel, msg.ToArray());
            }

            Debug.Log($"@Server -> User[ {connectionId} , {meUser.UserName} , {meUser.TeamNumber} ] Registered");
        }

        private void OnChatWhisper(int connectionId, ByteStream stream)
        {            
            int targetId = stream.PopInt32();
            stream.Append(connectionId);
            SendNetMessage(targetId, _reliableChannel, stream.ToArray());
        }

        private void OnChatBroadcast(int connectionId, ByteStream stream)
        {
            stream.Append(connectionId);
            BroadcastNetMessage(_reliableChannel, stream.ToArray(), connectionId);
        }

        private void BroadcastNetMessage(byte channel, byte[] data, int? excludeId = null)
        {
            foreach(var user in _NetUsers)
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

        private void OnChatMulticast(int connectionId, ByteStream stream)
        {
            stream.Append(connectionId);
            int[] targets = _NetUsers.Where(x => x.Value.TeamNumber == _NetUsers[connectionId].TeamNumber)
                                     .Select(x => x.Key)
                                     .ToArray();

            MulticastNetMessage(targets, _reliableChannel, stream.ToArray(), connectionId);
        }

        private void MulticastNetMessage(int[] targets, byte channel, byte[] data, int? excludeId = null)
        {
            foreach(var user in _NetUsers)
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

        // ----------- GUI DEBUG STUFF ----------

        private void OnGUI()
        {
            GUILayout.Space(32);
            GUILayout.Label("Online Users:");
            GUILayout.Space(32);

            foreach(var user in _NetUsers)
            {
                if(GUILayout.Button($"{user.Key} - {user.Value.UserName}"))
                {
                    // Kick Player
                    NetworkTransport.Disconnect(_socketId, user.Key, out var error);
                }
            }
        }
    }
}