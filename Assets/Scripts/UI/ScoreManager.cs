using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;
    public string gameMode;
    
    public HighScoreEntry(string name, int score, string mode)
    {
        playerName = name;
        this.score = score;
        gameMode = mode;
    }
}

public static class ScoreManager
{
    private const string HighScoresKey = "HighScores";
    private const string CurrentPlayerKey = "CurrentPlayerName";
    private const int MaxEntries = 10;
    private const int MaxNameLength = 5;
    
    public static List<HighScoreEntry> GetHighScores()
    {
        List<HighScoreEntry> scores = new List<HighScoreEntry>();
        
        string json = PlayerPrefs.GetString(HighScoresKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                string[] entries = json.Split('|');
                foreach (string entry in entries)
                {
                    if (string.IsNullOrEmpty(entry)) continue;
                    
                    string[] parts = entry.Split(',');
                    if (parts.Length >= 3)
                    {
                        scores.Add(new HighScoreEntry(
                            parts[0],
                            int.Parse(parts[1]),
                            parts[2]
                        ));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("ScoreManager: Error parsing high scores: " + e.Message);
            }
        }
        
        return scores;
    }
    
    public static void SaveHighScore(string playerName, int score, string gameMode)
    {
        if (string.IsNullOrEmpty(playerName) || score <= 0) return;
        
        List<HighScoreEntry> scores = GetHighScores();
        
        HighScoreEntry newEntry = new HighScoreEntry(
            playerName.Substring(0, Mathf.Min(playerName.Length, MaxNameLength)).ToUpper(),
            score,
            gameMode
        );
        
        scores.Add(newEntry);
        scores.Sort((a, b) => b.score.CompareTo(a.score));
        
        if (scores.Count > MaxEntries)
        {
            scores.RemoveAt(scores.Count - 1);
        }
        
        string json = "";
        foreach (HighScoreEntry entry in scores)
        {
            if (!string.IsNullOrEmpty(json)) json += "|";
            json += entry.playerName + "," + entry.score + "," + entry.gameMode;
        }
        
        PlayerPrefs.SetString(HighScoresKey, json);
        PlayerPrefs.Save();
        
        Debug.Log("ScoreManager: Saved high score - " + newEntry.playerName + ": " + newEntry.score + " (" + newEntry.gameMode + ")");
    }
    
    public static bool IsHighScore(int score)
    {
        List<HighScoreEntry> scores = GetHighScores();
        
        if (scores.Count < MaxEntries) return true;
        
        return score > scores[scores.Count - 1].score;
    }
    
    public static int GetMinHighScore()
    {
        List<HighScoreEntry> scores = GetHighScores();
        
        if (scores.Count == 0) return 0;
        if (scores.Count < MaxEntries) return 0;
        
        return scores[scores.Count - 1].score;
    }
    
    public static void SetCurrentPlayerName(string name)
    {
        PlayerPrefs.SetString(CurrentPlayerKey, name.Substring(0, Mathf.Min(name.Length, MaxNameLength)).ToUpper());
        PlayerPrefs.Save();
    }
    
    public static string GetCurrentPlayerName()
    {
        return PlayerPrefs.GetString(CurrentPlayerKey, "P1");
    }
}
