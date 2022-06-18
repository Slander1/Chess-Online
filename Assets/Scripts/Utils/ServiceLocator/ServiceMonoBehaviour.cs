using System.Threading.Tasks;
using ServiceLocator;
using UnityEngine;

namespace Utils.ServiceLocator
{
    public class ServiceMonoBehaviour: MonoBehaviour, IService 
    {
        private void Awake()
        {
            ServiceL.Register(this);
        }

        private void OnDestroy()
        {
            ServiceL.Unregister(this);
        }

    }
}