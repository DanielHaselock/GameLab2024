using System;
using System.Dynamic;
using UnityEngine;

namespace Networking.Utils
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        
        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (Instance == null)
                Instance = this as T;
            
            if(Instance != null && Instance != this)
                Destroy(this.gameObject);
        }
    }
}