#define DEBUG_VerboseConsoleLogging

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// This is a static class, so everything in it must be static
static public class SaveGameManager
{
    // ———————————————— Statics ———————————————— //
    // I've chosen not to use SNAKE_CASE for statics in this class because everything is static.
    static private SaveFile saveFile;
    static private string   filePath;
    // LOCK, if true, prevents the game from saving. This avoids issues that can
    //  happen while loading files.
    static public bool LOCK
    {
        get;
        private set;
    }


    static SaveGameManager()
    {
        LOCK = false;
        filePath = Application.persistentDataPath + "/AsteraX.save";

#if DEBUG_VerboseConsoleLogging
        Debug.Log("SaveGameManager:Awake() – Path: " + filePath);
#endif

        saveFile = new SaveFile();
    }


    static public void Save()
    {
        // If this is LOCKed, don't save
        if (LOCK) return;

        saveFile.stepRecords = AchievementManager.GetStepRecords();
        saveFile.achievements = AchievementManager.GetAchievements();

        string jsonSaveFile = JsonUtility.ToJson(saveFile, true);

        File.WriteAllText(filePath, jsonSaveFile);

#if DEBUG_VerboseConsoleLogging
        Debug.Log("SaveGameManager:Save() – Path: " + filePath);
        Debug.Log("SaveGameManager:Save() – JSON: " + jsonSaveFile);
#endif
    }


    static public void Load()
    {
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
#if DEBUG_VerboseConsoleLogging
            Debug.Log("SaveGameManager:Load() – File text is:\n" + dataAsJson);
#endif

            try
            {
                saveFile = JsonUtility.FromJson<SaveFile>(dataAsJson);
            }
            catch
            {
                Debug.LogWarning("SaveGameManager:Load() – SaveFile was malformed.\n" + dataAsJson);
                return;
            }

#if DEBUG_VerboseConsoleLogging
            Debug.Log("SaveGameManager:Load() – Successfully loaded save file.");
#endif
            LOCK = true;
            // Load the Achievements
            AchievementManager.LoadDataFromSaveFile(saveFile);

            LOCK = false;
        }
        else
        {

#if DEBUG_VerboseConsoleLogging
            Debug.LogWarning("SaveGameManager:Load() – Unable to find save file. "
                + "This is totally fine if you've never gotten a game over "
                + "or completed an Achievement, which is when the game is saved.");
#endif

        }
    }


    static public void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            saveFile = new SaveFile();
            Debug.Log("SaveGameManager:DeleteSave() – Successfully deleted save file.");
        }
        else
        {
            Debug.LogWarning("SaveGameManager:DeleteSave() – Unable to find and delete save file!"
                + " This is absolutely fine if you've never saved or have just deleted the file.");
        }

        AchievementManager.ClearStepsAndAchievements();
    }


    static internal bool CheckHighScore(int score)
    {
        if (score > saveFile.highScore)
        {
            saveFile.highScore = score;
            return true;
        }
        return false;
    }
}



// A class must be serializable to be converted to and from JSON by JsonUtility.
//[System.Serializable]
public class SaveFile
{
    public StepRecord[] stepRecords;
    public Achievement[] achievements;
    public int highScore = 5000;
}