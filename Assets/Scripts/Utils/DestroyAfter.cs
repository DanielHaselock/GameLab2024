using System;
using UnityEngine;

namespace Utils
{
    public class DestroyAfter : MonoBehaviour
    {
        [SerializeField] private float delay = 1;
        private void Start()
        {
            Destroy(this.gameObject, delay);
        }
    }
}