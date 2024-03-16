using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Slider _hpBar;
    [SerializeField] string abc;

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        _hpBar.value = currentHealth / maxHealth;
    }
}
