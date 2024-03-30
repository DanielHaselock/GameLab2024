using System;
using UnityEngine;

namespace Networking.Utils
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}