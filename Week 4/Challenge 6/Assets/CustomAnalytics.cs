using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public static class CustomAnalytics
{
    public static void SendAchievementUnlocked(Achievement ach)
    {
        AnalyticsEvent.AchievementUnlocked(ach.name, new Dictionary<string, object>
        {
            {"time",DateTime.Now}
        });
    }

    public static void SendLevelStart(int level)
    {
        AnalyticsEvent.LevelStart(level, new Dictionary<string, object>
        {
            {"time", DateTime.Now}
        });
    }

    public static void SendGameOver()
    {
        AnalyticsEvent.GameOver(null, new Dictionary<string, object>
        {
            {"time", DateTime.Now},
            {"score",AsteraX.SCORE},
            {"level",AsteraX.GAME_LEVEL},
            {"gotHighScore",AsteraX.GOT_HIGH_SCORE}
            
        });
    }

    public static void SendFinalShipPartChoice()
    {
        Dictionary<string,object> dict=new Dictionary<string, object>();
        dict.Add("time",DateTime.Now);
        int num;
        
        //This is designed to accommodate additional eShipPartTypes
        foreach (ShipPart.eShipPartType type in (ShipPart.eShipPartType[])
            Enum.GetValues(typeof(ShipPart.eShipPartType)))
        {
            if (dict.Count == 10)
            {
                //This is necessary because AnalyticsEvent.Custom has a hard 
                //Limit on this dict size of 10, according to
                //https://docs.unityed.com/Manual/UnityAnalyticsCustomEventsSDK.html
                break;
            }

            num = ShipCustomizationPanel.GetSelectedPart(type);
            dict.Add(type.ToString(),num);
        }

        AnalyticsEvent.Custom("ShipPartChoice", dict);
    }
}
