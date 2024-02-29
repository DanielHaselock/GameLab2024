using Fusion;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public interface Damage
{
    public void Attack(HealthComponent other);
}

public class DamageComponent : MonoBehaviour, Damage
{
    public float AttackDamage;

    [SerializeField] private List<HealthComponent> hittableObjects = new List<HealthComponent>();

    HealthComponent selfheal;


    private void Start()
    {
        selfheal = transform.parent.GetComponentInChildren<HealthComponent>();
    }

    public void Attack(HealthComponent other)
    {
        Debug.Log("REMOVING HEALTH");
        other.RemoveHealth(AttackDamage);
    }
    public void InputAttack()
    {
        List<HealthComponent> healthtoDestroy = new List<HealthComponent> ();

        foreach (HealthComponent obj in hittableObjects)
        {
            if (!obj)
            {
                healthtoDestroy.Add(obj);
                continue;
            }

            Attack(obj);
        }

        foreach (HealthComponent obj in healthtoDestroy)
        {
            hittableObjects.Remove(obj);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HealthComponent>() != transform.parent.GetComponentInChildren<HealthComponent>())
        {
            hittableObjects.Add(other.GetComponent<HealthComponent>());
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HealthComponent>() != transform.parent.GetComponentInChildren<HealthComponent>())
        {
            hittableObjects.Remove(other.GetComponent<HealthComponent>());
        }
    }
}
