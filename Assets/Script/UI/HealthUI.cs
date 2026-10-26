using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite filledHeart;
    [SerializeField] private Sprite emptyHeart;

    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = filledHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }
}