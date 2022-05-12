using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance { get; set; }
    public NetworkDriver driver;
    public Action connectionDropped;
    
    private NetworkConnection _connesction;

    private bool _isActive = false;
    private const float KeepAliveTickRate = 20.0f;
    private float _lastKeepAlive;
    private void Awake()
    {
        Instance = this;
    }
    
    public void Init(string ip,ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
        
        _connesction = driver.Connect(endpoint);
        
        Debug.Log("Attemping to cionnetct to Server on "+endpoint.Address);
        _isActive = true;

        RegisterToEvent();
    }

    public void ShutDown()
    {
        if (_isActive)
        {
            UnregisterToEvent();
            driver.Dispose();
            _isActive = false;
            _connesction = default(NetworkConnection);
        }
    }

    public void OnDestroy()
    {
        ShutDown();
    }
    
    public void Update()
    {
        if (!_isActive)
            return;

        driver.ScheduleUpdate().Complete();

        CheckAlive();
        
        UpdateMessagePump();
    }

    private void CheckAlive()
    {
        if (!_connesction.IsCreated && _isActive)
        {
            Debug.Log("Something went wrong, lost connection to server");
            connectionDropped?.Invoke();
            ShutDown();
        }
    }

    private void UpdateMessagePump()
    {
        NetworkEvent.Type cmd;
        while ((cmd = _connesction.PopEvent(driver, out var streamReader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                SendToServer(new NetWelcome());
                Debug.Log("We're connected");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(streamReader, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client god disconected from server");
                _connesction = default(NetworkConnection);
                connectionDropped?.Invoke();
                ShutDown();
            }
        }
    }

    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter streamWriter;
        driver.BeginSend(_connesction, out streamWriter);
        msg.Serialize(ref streamWriter);
        driver.EndSend(streamWriter);
    }

    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }
    
    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    } 
    
    private void OnKeepAlive(NetMessage netMessage)
    {
        SendToServer(netMessage);
    }
    
    
    




}
