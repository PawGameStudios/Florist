namespace Florist.Merge
{
using UnityEngine;
using System;

[System.Serializable]
public class ProductionJob
{
    public RecipeData recipe;
    public DateTime startTime;
    public DateTime endTime;
    public bool isCollected = false;
    public string kioskId; // Hangi kiosk'ta üretim yapıldığı

    public ProductionJob(RecipeData recipeData, string kioskId = "")
    {
        recipe = recipeData;
        startTime = DateTime.UtcNow;
        endTime = startTime.AddSeconds(Mathf.Max(0f, recipeData.productionTime));
        this.kioskId = kioskId;
    }

    public bool IsCompleted => DateTime.UtcNow >= endTime;
    public bool IsReadyToCollect => IsCompleted && !isCollected;

    public float GetProgress()
    {
        var totalTime = (float)(endTime - startTime).TotalSeconds;
        var elapsedTime = (float)(DateTime.UtcNow - startTime).TotalSeconds;
        return totalTime <= 0f ? 1f : Mathf.Clamp01(elapsedTime / totalTime);
    }

    public string GetRemainingTime()
    {
        if (IsCompleted && !isCollected) return "Topla!";
        if (isCollected) return "Tamamlandı!";

        var remaining = endTime - DateTime.UtcNow;
        int hours = (int)remaining.TotalHours;
        int minutes = remaining.Minutes;
        int seconds = remaining.Seconds;

        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }
}

}


