using ServiceLocator;
using UI;
using Unity.VisualScripting;
using Utils.ServiceLocator;

namespace Net
{
    public class NetLogic : IService
    {
        public Server server;
        public Client client;

        public void Init()
        {
            RegisterEvent();
        }

        private void RegisterEvent()
        {
            ServiceL.Get<Buttons>().setLocaleGame += InitClientServer;
            ServiceL.Get<Buttons>().shutDown += ShutDown;
        }

        private void ShutDown()
        {
            server.ShutDown();
            client.ShutDown();
        }

        private void UnregisterEvent()
        {
            ServiceL.Get<Buttons>().setLocaleGame -= InitClientServer;
        }

        private void InitClientServer(bool value, bool isServer)
        {
            server.Init(8007);
            client.Init("127.0.0.1", 8007);
        }

        
        
    }
}