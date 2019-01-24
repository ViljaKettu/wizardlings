using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    //const
    private const int MAX_CONNECTIONS = 10;
    private const string SERVER_IP = "127.0.0.1";
    private const int SERVER_PORT = 8999;
    private const int SERVER_WEB_PORT = 8998;
    private const int BUFFER_SIZE = 1024;

    //Channels
    private int reliableChannelId;   // Purchase an item
    private int unreliableChennelId; // Updating movement of the players

    // Host
    private int hostId;
    private int webHostId;

    // Logic
    private byte[] buffer = new byte[BUFFER_SIZE];
    private bool isInit = false;

    private void Start()
    {
        GlobalConfig config = new GlobalConfig();
        NetworkTransport.Init(config);

        // Host Topology
        ConnectionConfig cc = new ConnectionConfig();
        reliableChannelId = cc.AddChannel(QosType.Reliable);
        unreliableChennelId = cc.AddChannel(QosType.Unreliable);
        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

        // Adding hosts
        hostId = NetworkTransport.AddHost(topo, SERVER_PORT);
        webHostId = NetworkTransport.AddWebsocketHost(topo, SERVER_WEB_PORT);

        isInit = true;
        Debug.Log("Starting Server");
    }

    private void Update()
    {
        if(!isInit)
        {
            return;
        }

        int outHostId, outConnectionId, outChannelId;
        int receivedSize;
        byte error;

        NetworkEventType e = NetworkTransport.Receive(out outHostId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);

        if(e != NetworkEventType.Nothing)
        {
            // There is no message, let's stop here
            return;
        }

        switch(e)
        {
            case NetworkEventType.ConnectEvent:
                {
                    Debug.Log("Connection from " + outConnectionId + " through the channel " + outChannelId);
                    break;
                }
            case NetworkEventType.DisconnectEvent:
                {
                    Debug.Log("Connection from " + outConnectionId + " through the channel " + outChannelId);
                    break;
                }
            case NetworkEventType.DataEvent:
                {
                    Debug.Log("Data from " + outConnectionId + " through the channel " + outChannelId + "::: Message :::" + buffer);
                    break;
                }
            case NetworkEventType.BroadcastEvent:
                {
                    break;
                }
            case NetworkEventType.Nothing:
                {
                    return;
                }
            default:
                Debug.Log("Unknown network message type received: " + e);
                break;
        }
    }
}
