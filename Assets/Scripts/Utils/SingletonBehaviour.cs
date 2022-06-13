using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {

        private static readonly object Instancelock = new object();
        private static T _instance;
        
        public static T Instance
        {
            get
            {
                lock (Instancelock)
                {
                    if (_instance == null)
                    {
                        var t = FindObjectOfType<T>();
                        _instance = t;
                    }
                    return _instance;
                }
            }
        }
        private void Awake()
        {
            if (_instance != null && _instance != (T)this)
                Destroy(gameObject);
            else
                _instance = (T)this;
        }
    }
}