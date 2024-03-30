using System;
using UnityEngine;

namespace Networking.Utils
{
    public class Billboard : MonoBehaviour
    {
        private Transform lookTarget;
        private void Start()
        {
            lookTarget = Camera.main.transform;
        }

        private void Update()
        {
            var dir = lookTarget.position - transform.position;
            transform.forward = dir;
        }
    }
}