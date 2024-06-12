using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerInput> connectedPlayers = new List<PlayerInput>();
    public int maxPlayers = 4;
    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;

    void Awake()
    {
        // Verify that PlayerInputManager is initialized
        if (PlayerInputManager.instance == null)
        {
            Debug.LogError("PlayerInputManager instance is null during Awake!");
        }
    }

    void Start()
    {
        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined += AssignPlayer;
            PlayerInputManager.instance.onPlayerLeft += RemovePlayer;
        }
        else
        {
            Debug.LogError("PlayerInputManager instance is null!");
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
        }
        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("No player prefabs assigned!");
        }
    }

    void AssignPlayer(PlayerInput playerInput)
    {
        if (connectedPlayers.Count < maxPlayers)
        {
            connectedPlayers.Add(playerInput);
            int playerIndex = connectedPlayers.IndexOf(playerInput);

            if (playerIndex < playerPrefabs.Length)
            {
                GameObject playerPrefab = playerPrefabs[playerIndex];
                Transform spawnPoint = GetRandomSpawnPoint();

                if (spawnPoint != null)
                {
                    Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
                    Debug.Log($"Player {playerIndex + 1} spawned at {spawnPoint.position}");
                }
                else
                {
                    Debug.LogError("Spawn point is null!");
                }
            }
            else
            {
                Debug.LogError("Not enough player prefabs assigned for the number of players!");
            }
        }
        else
        {
            Debug.LogWarning("Maximum players reached!");
        }
    }

    void RemovePlayer(PlayerInput playerInput)
    {
        connectedPlayers.Remove(playerInput);
    }

    Transform GetRandomSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomIndex];
        }
        else
        {
            Debug.LogError("No spawn points available!");
            return null;
        }
    }
}
