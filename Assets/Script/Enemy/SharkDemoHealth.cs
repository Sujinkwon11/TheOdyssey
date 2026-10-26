using UnityEngine;
using UnityEngine.Events;

// A small test health component; this does not provide player movement.
[DisallowMultipleComponent]
public class SharkDemoHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private UnityEvent onDeath = new UnityEvent();

    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"{name}: took {amount} damage. Health: {currentHealth}/{maxHealth}", this);
        if (IsDead)
        {
            Debug.Log($"{name}: defeated. Connect On Death to your respawn/death system.", this);
            onDeath.Invoke();
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = Mathf.Max(1, maxHealth);
    }
}
