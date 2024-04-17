using GameLoop;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;

    private Image _healthBar;

    private void Start()
    {
        _healthBar = GetComponent<Image>();
    }

    private void Update()
    {
        if(!GameManager.instance.gameStarted)
            return;
        
        if(healthComponent != null && healthComponent.IsInitialised)
            _healthBar.fillAmount = (float)healthComponent.Health / healthComponent.MaxHealth;
    }
}
