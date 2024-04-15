using UnityEngine;

namespace GameLoop
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private float damage=1;
        [SerializeField] private float chargedamage = 2;
        public float Damage => damage;
        public float ChargeDamage => chargedamage;
    }
}