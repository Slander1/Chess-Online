using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLocator;

namespace Utils.ServiceLocator
{
    
    public static class ServiceL
    {
        private static readonly Dictionary<Type, IService> _itemServiceLocator 
            = new Dictionary<Type, IService>();
        
        

        public static void Register<T>(T newService) where T : IService
        {
            var type = newService.GetType();
            if (_itemServiceLocator.ContainsKey(type))
                return;
        
            _itemServiceLocator[type] = newService;
        }

        public static void Unregister<T>(T newService) where T : IService
        {
            var type = newService.GetType();
            if (_itemServiceLocator.ContainsKey(type))
                _itemServiceLocator.Remove(type);
        }

        public static T Get<T>() where T : IService
        {
            var type = typeof(T);
            return (T)_itemServiceLocator[type];
        }

        public static async Task WaitInitialize()
        {
            await Task.Delay(10000);
       }
    }
}
