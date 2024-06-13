using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class gameController : MonoBehaviour
{
    public static gameController instance; // Singleton instance

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
        playerScores = new int[maxPlayers];
        StartRound();
    }

    void StartRound()
    {
        // Clear existing players and bots
        foreach (GameObject player in players)
        {
            Destroy(player);
        }
        players.Clear();

        for (int i = 0; i < maxPlayers; i++)
        {
            Vector3 spawnPosition = spawnPoints[i].position;
            GameObject playerObj = Instantiate(i == 0 ? playerPrefab : botPrefab, spawnPosition, Quaternion.identity);
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

    public void BotDied(GameObject bot)
    {
        players.Remove(bot);
        Destroy(bot);

        // Ensure max 4 bots at all times
        if (players.Count < maxPlayers)
        {
            RespawnBot();
        }

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

        // Start new round
        StartRound();
    }

    void EndGame()
    {
        Debug.Log("Game Over! Player " + (System.Array.IndexOf(playerScores, pointsToWin) + 1) + " wins!");
        // Handle game over logic (e.g., show UI, reset game, etc.)
    }

    void RespawnBot()
    {
        Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
        GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
        players.Add(botObj);
    }

    public void SpawnEnemy(InputAction.CallbackContext context)
    {
        if (context.performed && players.Count < maxPlayers)
        {
            Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
            GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
            players.Add(botObj);
        }
    }
}

