using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth = 3;

    [SerializeField] private float damageCooldown = 2f;
    [SerializeField] private float blinkDuration = 1f;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private HealthUI healthUI;

    private bool canTakeDamage = true;
    private Renderer[] renderers;

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        if (healthUI == null)
        {
            healthUI = FindFirstObjectByType<HealthUI>();
        }

        healthUI.UpdateHealth(currentHealth);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Damage damage = hit.collider.GetComponentInParent<Damage>();

        if (damage != null)
        {
            TakeDamage(damage.damageAmount);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Damage damage = other.GetComponentInParent<Damage>();

        if (damage != null)
        {
            TakeDamage(damage.damageAmount);
        }
    }

    public void TakeDamage(int amount)
    {
        if (!canTakeDamage)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        healthUI.UpdateHealth(currentHealth);

        Debug.Log("Remaining Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            GameOver();
            return;
        }

        StartCoroutine(DamageCooldown());
        StartCoroutine(Blink());
    }

    public void AddHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Health: " + currentHealth);

        healthUI.UpdateHealth(currentHealth);
    }

    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;

        yield return new WaitForSeconds(damageCooldown);

        canTakeDamage = true;
    }

    private IEnumerator Blink()
    {
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            SetRenderers(false);
            yield return new WaitForSeconds(blinkInterval);

            SetRenderers(true);
            yield return new WaitForSeconds(blinkInterval);

            elapsed += blinkInterval * 2f;
        }

        SetRenderers(true);
    }

    private void SetRenderers(bool visible)
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER");
    }
}