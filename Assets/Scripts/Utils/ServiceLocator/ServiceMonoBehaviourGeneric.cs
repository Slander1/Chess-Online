using System;
using System.Threading.Tasks;
using ServiceLocator;
using UnityEngine;

namespace Utils.ServiceLocator
{
    public class ServiceMonoBehaviourGeneric<T>: MonoBehaviour where T: IService, new ()
    {

        protected T data { get; private set; }

        private async void Awake()
        {
            data = new T();
            ServiceL.Register(data);
        }

        private void OnDestroy()
        {
            ServiceL.Unregister(data);
        }
    }
}