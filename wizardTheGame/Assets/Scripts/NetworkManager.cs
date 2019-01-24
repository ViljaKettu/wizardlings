using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public bool isAtStartup = true;
    NetworkClient myClient;

    private void Update()
    {
        if(isAtStartup)
        {
            if(Input.GetKeyDown(KeyCode.S))
            {
                SetupServer();
            }
            if(Input.GetKeyDown(KeyCode.C))
            {
                SetupClient();
            }
            if(Input.GetKeyDown(KeyCode.B))
            {
                SetupServer();
                SetupLocalClient();
            }
        }
    }

    private void OnGUI()
    {
        if(isAtStartup)
        {
            GUI.Label(new Rect(2, 10, 150, 100), "Press S for server");
            GUI.Label(new Rect(2, 10, 150, 100), "Press C for client");
            GUI.Label(new Rect(2, 30, 150, 100), "Press B for both");
        }
    }

    public void SetupServer()
    {
        NetworkServer.Listen(4444);
        isAtStartup = false;
        Debug.Log("Server setup");
    }

    public void SetupClient()
    {
        myClient = new NetworkClient();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.Connect("127.0.0.1", 4444);
        isAtStartup = false;
        Debug.Log("Client setup");
    }

    public void SetupLocalClient()
    {
        myClient = ClientScene.ConnectLocalServer();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        isAtStartup = false;
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
    }
}
