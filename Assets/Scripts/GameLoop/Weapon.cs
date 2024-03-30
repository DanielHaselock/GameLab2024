using UnityEngine;

namespace GameLoop
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private float damage=1;
        public float Damage => damage;
    }
}