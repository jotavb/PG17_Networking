using UnityEngine.Networking;
using UnityEngine;
using System;
using System.Collections;

[System.Obsolete]
public class LLClient : MonoBehaviour
{
    [SerializeField] private string _serverAdress = "127.0.0.1";
    [SerializeField] private int _serverPort = 27000;
    [SerializeField] private int _bufferSize = 1024;
    [SerializeField] private byte _threadPoolSize = 3;

    private byte _reliableChannel;
    private byte _unreliableChannel;
    private int _socketId = 0;
    private int _serverConnectionId = 0;
    
    private float _immediatePing;
    private float _totalPing;
    private int _pingCount;

    private void Start()
    {
        ConnectToServer();
        InvokeRepeating("Pinger", 1f, 1f);
    }

    private void ConnectToServer()
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
        _socketId = NetworkTransport.AddHost(hostTopology, 0);

        byte error;
        _serverConnectionId = NetworkTransport.Connect(_socketId, _serverAdress, _serverPort, 0, out error);

        if(error != 0)
        {
            Debug.LogError($"Error: Connecting to server -> [{error}]");
        }
        else
        {
            StartCoroutine(Receiver());
            Debug.Log($"Connect to Server -> {_socketId}");
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
                        // Echo Client
                        long timeSpan = DateTime.UtcNow.Ticks - BitConverter.ToInt64(recBuffer, 0);
                        _immediatePing = (float)(timeSpan/100000);  
                        _totalPing += _immediatePing;
                        ++_pingCount;          
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
                        Debug.LogError($"Receiver -> Unrecognized Message Type [{netEventType.ToString()}]");
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

    private void Pinger()
    {
        byte[] data = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        byte error;

        NetworkTransport.Send
        (
            _socketId,
            _serverConnectionId,
            _reliableChannel,
            data,
            data.Length,
            out error
        );

        if(error != 0)
        {
            Debug.LogError($"Pinger -> Error Sending Message [{error}]");
        }
    }

    private void OnGUI()
    {
        float avgPing = _totalPing / (float)_pingCount;
        GUILayout.Label($"    [ {avgPing} ]  -  [ {_immediatePing} ]");
    }
}
