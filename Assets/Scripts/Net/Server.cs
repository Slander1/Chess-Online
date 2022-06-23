using System;
using Net.NetMassage;
using ServiceLocator;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using Utils.ServiceLocator;

namespace Net
{
    public class Server : ServiceMonoBehaviour
    {
        public NetworkDriver driver;
        public Action connectionDropped;
    
        private NativeList<NetworkConnection> _connesctions;

        private bool _isActive = false;
        private const float KeepAliveTickRate = 20.0f;
        private float _lastKeepAlive;
        

        public void Init(ushort port)
        {
            ServiceL.Register(this);
            driver = NetworkDriver.Create();
            NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = port;

            if (driver.Bind(endpoint) != 0)
            {
                Debug.Log("Unable to bind on port "+ endpoint.Port);   
                return;
            }
            else
            {
                driver.Listen();
                Debug.Log("Currently listening on port "+ endpoint.Port); 
            
            }
        
            _connesctions = new NativeList<NetworkConnection>(2, Allocator.Persistent);
            _isActive = true;
        }

        public void ShutDown()
        {
            if (_isActive)
            {
                driver.Dispose();
                _connesctions.Dispose();
                _isActive = false;
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

            KeepAlive();
        
            driver.ScheduleUpdate().Complete();
            CleanupConnections();
            AcceptNewConnections();
            UpdateMessagePump();
        }

        private void KeepAlive()
        {
            if (Time.time - _lastKeepAlive > KeepAliveTickRate)
            {
                _lastKeepAlive = Time.time;
                Broadcast(new NetKeepAlive());
            }
        }

        private void UpdateMessagePump()
        {
            //DataStreamReader streamReader;
            for (int i = 0; i < _connesctions.Length; i++)
            {
                NetworkEvent.Type cmd;
                while ((cmd = driver.PopEventForConnection(_connesctions[i],  out var streamReader) )!= NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        NetUtility.OnData(streamReader, _connesctions[i], this);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconected from server");
                        _connesctions[i] = default(NetworkConnection);
                        connectionDropped?.Invoke();
                        ShutDown();
                    }
                }
            }
        }

        private void AcceptNewConnections()
        {
            NetworkConnection networkConnection;
            while ((networkConnection = driver.Accept()) != default(NetworkConnection))
                _connesctions.Add(networkConnection);
        }

        private void CleanupConnections()
        {
            for (int i = 0; i < _connesctions.Length; i++)
            {
                if (!_connesctions[i].IsCreated)
                {
                    _connesctions.RemoveAtSwapBack(i);
                    --i;
                }
            }
        }
    
        public void SendToClient(NetworkConnection connection, NetMessage msg)
        {
            driver.BeginSend(connection, out var streamWriter);
            msg.Serialize(ref streamWriter);
            driver.EndSend(streamWriter);

        }

        public void Broadcast(NetMessage msg)
        {
            for (int i = 0; i < _connesctions.Length; i++)
            {
                if (_connesctions[i].IsCreated)
                {
                    //Debug.Log($"Sending {msg.Code} to : {connesctions[i].InternalId}");
                    SendToClient(_connesctions[i], msg);
                }
            }
        }
    
    

    
    }
}
