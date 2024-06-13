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
    private bool spawnEnemyPressed = false;

    void Awake()
    {
        Debug.Log("Awake called");
        if (instance == null)
        {
            instance = this;
            Debug.Log("Instance set to this");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            Debug.Log("Duplicate instance destroyed");
        }

        DontDestroyOnLoad(gameObject);
        Debug.Log("DontDestroyOnLoad called");
    }

    void Start()
    {
        Debug.Log("Start called");
        // Initialize spawn points (you need to set this up in your scene)
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
            Debug.Log("Spawn point added: " + child.name);
        }

        StartGame();
        StartCoroutine(AutoSpawnEnemy());
    }

    void StartGame()
    {
        Debug.Log("StartGame called");
        playerScores = new int[maxPlayers];
        StartRound();
    }

    void StartRound()
    {
        Debug.Log("StartRound called");
        // Clear existing players and bots
        foreach (GameObject player in players)
        {
            Destroy(player);
            Debug.Log("Player destroyed: " + player.name);
        }
        players.Clear();
        Debug.Log("Players list cleared");

        for (int i = 0; i < maxPlayers; i++)
        {
            Vector3 spawnPosition = spawnPoints[i].position;
            GameObject playerObj = Instantiate(i == 0 ? playerPrefab : botPrefab, spawnPosition, Quaternion.identity);
            players.Add(playerObj);
            Debug.Log("Player added: " + playerObj.name + " at position " + spawnPosition);
        }
    }

    public void PlayerDied(GameObject player)
    {
        Debug.Log("PlayerDied called for: " + player.name);
        players.Remove(player);
        Destroy(player);

        // Check if round needs to end
        if (players.Count == 1)
        {
            Debug.Log("Only one player left, ending round");
            EndRound();
        }
    }

    public void BotDied(GameObject bot)
    {
        Debug.Log("BotDied called for: " + bot.name);
        players.Remove(bot);
        Destroy(bot);

        // Ensure max 4 bots at all times
        if (players.Count < maxPlayers)
        {
            Debug.Log("Players count less than maxPlayers, respawning bot");
            RespawnBot();
        }

        // Check if round needs to end
        if (players.Count == 1)
        {
            Debug.Log("Only one player left, ending round");
            EndRound();
        }
    }

    void EndRound()
    {
        Debug.Log("EndRound called");
        int winnerIndex = players.IndexOf(players[0]); // Assuming only one player left
        Debug.Log("Winner index: " + winnerIndex);

        // Update score
        playerScores[winnerIndex]++;
        Debug.Log("Player " + winnerIndex + " score updated to " + playerScores[winnerIndex]);

        // Check if game should end
        if (playerScores[winnerIndex] >= pointsToWin)
        {
            Debug.Log("Player " + winnerIndex + " reached points to win");
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
        Debug.Log("RespawnBot called");
        Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
        GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
        players.Add(botObj);
        Debug.Log("Bot respawned: " + botObj.name + " at position " + spawnPosition);
    }

    public void SpawnEnemy(InputAction.CallbackContext context)
    {
        Debug.Log("SpawnEnemy called with context: " + context);
        if (context.performed && players.Count < maxPlayers)
        {
            spawnEnemyPressed = true;
            Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
            GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
            players.Add(botObj);
            Debug.Log("Enemy spawned: " + botObj.name + " at position " + spawnPosition);
        }
    }

    IEnumerator AutoSpawnEnemy()
    {
        Debug.Log("AutoSpawnEnemy coroutine started");
        yield return new WaitForSeconds(5f);

        if (!spawnEnemyPressed && players.Count < maxPlayers)
        {
            Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
            GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
            players.Add(botObj);
            Debug.Log("Auto spawned enemy: " + botObj.name + " at position " + spawnPosition);
        }
    }
}

