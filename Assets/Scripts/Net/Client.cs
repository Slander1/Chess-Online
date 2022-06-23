using System;
using Net.NetMassage;
using ServiceLocator;
using Unity.Networking.Transport;
using UnityEngine;
using Utils.ServiceLocator;

namespace Net
{
    public class Client : ServiceMonoBehaviour
    {
        public NetworkDriver driver;
        public Action connectionDropped;
    
        private NetworkConnection _connection;

        private bool _isActive = false;
        private float _lastKeepAlive;

        public void Init(string ip,ushort port)
        {
            ServiceL.Register(this);
            driver = NetworkDriver.Create();
            NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
        
            _connection = driver.Connect(endpoint);
        
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
                _connection = default(NetworkConnection);
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
            if (!_connection.IsCreated && _isActive)
            {
                Debug.Log("Something went wrong, lost connection to server");
                connectionDropped?.Invoke();
                ShutDown();
            }
        }

        private void UpdateMessagePump()
        {
            NetworkEvent.Type cmd;
            while ((cmd = _connection.PopEvent(driver, out var streamReader)) != NetworkEvent.Type.Empty)
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
                    _connection = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    ShutDown();
                }
            }
        }

        public void SendToServer(NetMessage msg)
        {
            DataStreamWriter streamWriter;
            driver.BeginSend(_connection, out streamWriter);
            msg.Serialize(ref streamWriter);
            driver.EndSend(streamWriter);
        }

        private void RegisterToEvent()
        {
            NetUtility.CKeepAlive += OnKeepAlive;
        }
    
        private void UnregisterToEvent()
        {
            NetUtility.CKeepAlive -= OnKeepAlive;
        } 
    
        private void OnKeepAlive(NetMessage netMessage)
        {
            SendToServer(netMessage);
        }
    }
}
