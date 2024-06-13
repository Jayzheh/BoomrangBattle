using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton instance

    public GameObject playerPrefab; // Prefab for the player
    public GameObject botPrefab; // Prefab for the bot

    public int maxPlayers = 4;
    public int pointsToWin = 8;

    private List<Transform> spawnPoints = new List<Transform>(); // List of spawn points
    private List<GameObject> players = new List<GameObject>(); // List of active players
    private int[] playerScores; // Array to track player scores

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialize spawn points (you need to set this up in your scene)
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }

        StartGame();
    }

    void StartGame()
    {
        StartRound();
    }

    void StartRound()
    {
        playerScores = new int[maxPlayers];

        for (int i = 0; i < maxPlayers; i++)
        {
            Vector3 spawnPosition = spawnPoints[i].position;
            GameObject playerObj = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            players.Add(playerObj);
        }
    }

    public void PlayerDied(GameObject player)
    {
        players.Remove(player);
        Destroy(player);

        // Check if round needs to end
        if (players.Count == 1)
        {
            EndRound();
        }
    }

    void EndRound()
    {
        int winnerIndex = players.IndexOf(players[0]); // Assuming only one player left

        // Update score
        playerScores[winnerIndex]++;

        // Check if game should end
        if (playerScores[winnerIndex] >= pointsToWin)
        {
            EndGame();
            return;
        }

        // Respawn bots
        RespawnBots();

        // Start new round
        StartRound();
    }

    void EndGame()
    {
        Debug.Log("Game Over! Player " + (Array.IndexOf(playerScores, pointsToWin) + 1) + " wins!");
        // Handle game over logic (e.g., show UI, reset game, etc.)
    }

    void RespawnBots()
    {
        // Destroy existing bots
        foreach (GameObject bot in GameObject.FindGameObjectsWithTag("Bot"))
        {
            Destroy(bot);
        }

        // Respawn new bots
        for (int i = 1; i < maxPlayers; i++) // Start from 1 because player is at index 0
        {
            Vector3 spawnPosition = spawnPoints[i].position;
            GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
            players.Add(botObj);
        }
    }
}
