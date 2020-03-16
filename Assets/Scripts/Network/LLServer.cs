using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Obsolete]
public class LLServer : MonoBehaviour
{
    [SerializeField] private int _serverPort = 27000;
    [SerializeField] private int _bufferSize = 1024;
    [SerializeField] private byte _threadPoolSize = 3;

    private byte _reliableChannel;
    private byte _unreliableChannel;
    private int _socketId;

    private void Start()
    {
        StartServer();
    }

    private void StartServer()
    {
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
                        // Echo Server
                        NetworkTransport.Send(_socketId, recConnectionId, _reliableChannel, recBuffer, recDataSize, out error);              
                        break;
                    }
                    case NetworkEventType.ConnectEvent:
                    {
                        Debug.Log($"receiver.Connect -> Socket[{recSocketId}], userId[{recConnectionId}]");
                        break;
                    }
                    case NetworkEventType.DisconnectEvent:
                    {
                        Debug.Log($"receiver.Disconnect -> Socket[{recSocketId}], userId[{recConnectionId}]");
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
}
