using UnityEngine;
using System.Linq;
public static class LevelRepository
{
    private static LevelData[] cachedLevels;

    public static LevelData[] LoadAllLevels()
    {
        if (cachedLevels != null && cachedLevels.Length > 0)
            return cachedLevels;

        var loaded = Resources.LoadAll<LevelData>("Levels");
        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning("Resources/Levels içinde seviye bulunamadı!");
            return new LevelData[0];
        }

        cachedLevels = loaded.OrderBy(l => l.name).ToArray();
        return cachedLevels;
    }
    
    public static LevelData GetLevelByIndex(int index)
    {
        var levels = LoadAllLevels();
        if (index >= 0 && index < levels.Length) return levels[index];
        return null;
    }

    public static int TotalCount => cachedLevels != null ? 
        cachedLevels.Length : LoadAllLevels().Length;

}
