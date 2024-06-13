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

    // Specify the plane dimensions and center position
    private float planeWidth = 5f;
    private float planeLength = 5f;
    private Vector3 planeCenter = new Vector3(0f, -0.46f, 0f);

    private List<Vector3> spawnPoints = new List<Vector3>(); // List of spawn points
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
        GenerateSpawnPoints();
        StartGame();
        StartCoroutine(AutoSpawnEnemy());
    }

    void GenerateSpawnPoints()
    {
        Debug.Log("GenerateSpawnPoints called");
        spawnPoints.Clear();

        // Generate random spawn points within the specified plane
        for (int i = 0; i < maxPlayers; i++)
        {
            float randomX = Random.Range(-planeWidth / 2f, planeWidth / 2f);
            float randomZ = Random.Range(-planeLength / 2f, planeLength / 2f);
            Vector3 spawnPoint = planeCenter + new Vector3(randomX, 0f, randomZ);
            spawnPoints.Add(spawnPoint);
            Debug.Log("Spawn point added: " + spawnPoint);
        }
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

        for (int i = 0; i < maxPlayers && i < spawnPoints.Count; i++) // Ensure spawn points count is considered
        {
            Vector3 spawnPosition = spawnPoints[i];
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
        if (spawnPoints.Count > 0)
        {
            Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
            players.Add(botObj);
            Debug.Log("Bot respawned: " + botObj.name + " at position " + spawnPosition);
        }
        else
        {
            Debug.LogError("No spawn points available for respawning bot!");
        }
    }

    public void SpawnEnemy(InputAction.CallbackContext context)
    {
        Debug.Log("SpawnEnemy called with context: " + context);
        if (context.performed && players.Count < maxPlayers)
        {
            spawnEnemyPressed = true;
            if (spawnPoints.Count > 0)
            {
                Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)];
                GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
                players.Add(botObj);
                Debug.Log("Enemy spawned: " + botObj.name + " at position " + spawnPosition);
            }
            else
            {
                Debug.LogError("No spawn points available for spawning enemy!");
            }
        }
    }

    IEnumerator AutoSpawnEnemy()
    {
        Debug.Log("AutoSpawnEnemy coroutine started");
        yield return new WaitForSeconds(5f);

        if (!spawnEnemyPressed && players.Count < maxPlayers)
        {
            if (spawnPoints.Count > 0)
            {
                Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)];
                GameObject botObj = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
                players.Add(botObj);
                Debug.Log("Auto spawned enemy: " + botObj.name + " at position " + spawnPosition);
            }
            else
            {
                Debug.LogError("No spawn points available for auto-spawning enemy!");
            }
        }
    }
}

