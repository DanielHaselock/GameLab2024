using System;
using UnityEngine;

namespace Utils
{
    public class Move : MonoBehaviour
    {   
        [SerializeField] private Vector3 direction = Vector3.up;
        [SerializeField] private float speed=10;

        private void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);
        }
    }
}