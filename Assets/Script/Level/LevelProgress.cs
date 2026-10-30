
using UnityEngine;

public static class LevelProgress
{
    private const string SaveKey = "Odyssey_MaxUnlockedLevel";

    // Level 1 is unlocked by default
    public static int HighestUnlockedLevel
    {
        get
        {
            return Mathf.Max(1, PlayerPrefs.GetInt(SaveKey, 1));
        }
    }

    // Check if a level is unlocked
    public static bool IsUnlocked(int levelNumber)
    {
        return levelNumber >= 1 &&
               levelNumber <= HighestUnlockedLevel;
    }

    // Unlock the next level after completing one
    public static void CompleteLevel(int completedLevelNumber)
    {
        int nextLevel = completedLevelNumber + 1;

        if (nextLevel > HighestUnlockedLevel)
        {
            PlayerPrefs.SetInt(SaveKey, nextLevel);
            PlayerPrefs.Save();

            Debug.Log("Level " + nextLevel + " unlocked!");
        }
    }
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey("Odyssey_MaxUnlockedLevel");
        PlayerPrefs.Save();

        Debug.Log("[DEBUG] Level progress reset!");
    }
}
